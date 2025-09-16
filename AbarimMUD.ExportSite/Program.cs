using System.Collections.Generic;
using System.IO;
using System;
using MUDMapBuilder;
using AbarimMUD.Storage;
using System.Linq;
using AbarimMUD.Data;
using System.Drawing;
using static MUDMapBuilder.MMBProject;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace AbarimMUD.ExportAreasToMMB
{
	internal static class Program
	{
		class AreaData
		{
			public string Name { get; }
			public string Credits { get; }
			public string MinLevel { get; }
			public string MaxLevel { get; }

			public AreaData(string name, string credits, string minLevel, string maxLevel)
			{
				Name = name;
				Credits = credits;
				MinLevel = minLevel;
				MaxLevel = maxLevel;
			}
		}

		class EquipmentData
		{
			public string Name { get; }
			public string Slot { get; }
			public string Material { get; }
			public string Price { get; }
			public string EnchantmentTier { get; }
			public string Stats { get; }
			public List<string> Affects { get; } = new List<string>();
			public List<string> ForgeComponents { get; } = new List<string>();

			public EquipmentData(string name, string slot, string material, string price, string enchantmentTier, string stats)
			{
				Name = name;
				Slot = slot;
				Material = material;
				Price = price;
				EnchantmentTier = enchantmentTier;
				Stats = stats;
			}
		}

		class ConsumableData
		{
			public string Name { get; }
			public string Type { get; }
			public string Price { get; }
			public List<string> Affects { get; } = new List<string>();

			public ConsumableData(string name, string type, string price)
			{
				Name = name;
				Type = type;
				Price = price;
			}
		}

		class EnchantmentData
		{
			public string Name { get; }
			public string Stones { get; }
			public string Slots { get; }
			public string Materials { get; }
			public string Price { get; }
			public List<string> Affects { get; } = new List<string>();

			public EnchantmentData(string name, string stones, string slots, string materials, string price)
			{
				Name = name;
				Stones = stones;
				Slots = slots;
				Materials = materials;
				Price = price;
			}
		}


		static void Log(string message) => Console.WriteLine(message);

		static string DataFolder(string outputFolder) => Path.Combine(outputFolder, "_data");

		private static JsonSerializerOptions CreateJsonOptions()
		{
			var result = new JsonSerializerOptions
			{
				WriteIndented = true,
				IncludeFields = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
				IgnoreReadOnlyFields = false,
				IgnoreReadOnlyProperties = false,
			};

			result.Converters.Add(new JsonStringEnumConverter());
			result.Converters.Add(new ColorJsonConverter());

			return result;
		}

		static void ExportAreas(string outputFolder)
		{
			// Spawn mobiles to properly color corresponding rooms
			foreach (var area in Area.Storage)
			{
				foreach (var mobileReset in area.MobileResets)
				{
					var mobile = Mobile.GetMobileById(mobileReset.MobileId);
					if (mobile == null)
					{
						Log($"{area.Name}: Couldn't find mobile with id {mobileReset.MobileId}");
						continue;
					}

					var room = Room.GetRoomById(mobileReset.RoomId);
					if (room == null)
					{
						Log($"{area.Name}: Couldn't find room with id {mobileReset.RoomId}");
						continue;
					}

					// Spawn
					var newMobile = new MobileInstance(mobile, room);
				}
			}

			// Convert DikuLoad areas to MMB Areas
			// And build dict of all mobiles
			var areas = new List<MMBArea>();
			var allMobiles = new Dictionary<int, Mobile>();
			foreach (var dikuArea in Area.Storage)
			{
				if (dikuArea.Rooms == null || dikuArea.Rooms.Count == 0)
				{
					Console.WriteLine($"Warning: Area '{dikuArea.Name} has no rooms. Skipping.");
					continue;
				}

				areas.Add(dikuArea.ToMMBArea());

				foreach (var mobile in dikuArea.Mobiles)
				{
					if (allMobiles.ContainsKey(mobile.Id))
					{
						throw new Exception($"Dublicate mobile. New mobile: {mobile}. Old mobile: {allMobiles[mobile.Id]}");
					}

					allMobiles[mobile.Id] = mobile;
				}
			}

			// Build complete dictionary of rooms, mobiles and area exits
			var allRooms = new Dictionary<int, MMBRoom>();
			var allAreaExits = new Dictionary<int, MMBRoom>();
			foreach (var area in areas)
			{
				foreach (var room in area.Rooms)
				{
					if (allRooms.ContainsKey(room.Id))
					{
						throw new Exception($"Dublicate room id. New room: {room}. Old room: {allRooms[room.Id]}");
					}

					allRooms[room.Id] = room;

					var areaExit = room.Clone();

					areaExit.Name = $"To {area.Name} #{areaExit.Id}";
					areaExit.FrameColor = Color.Blue;
					areaExit.Color = Color.Blue;


					allAreaExits[room.Id] = areaExit;
				}
			}

			// Now add areas exits
			foreach (var area in areas)
			{
				var areaExits = new Dictionary<int, MMBRoom>();
				foreach (var room in area.Rooms)
				{
					foreach (var exit in room.Connections)
					{
						MMBRoom inAreaRoom;
						inAreaRoom = (from r in area.Rooms where r.Id == exit.Value.RoomId select r).FirstOrDefault();
						if (inAreaRoom != null)
						{
							continue;
						}

						areaExits[exit.Value.RoomId] = allAreaExits[exit.Value.RoomId];
					}
				}

				foreach (var pair in areaExits)
				{
					area.Add(pair.Value);
				}
			}

			// Finally add mobiles as content
			foreach (var dikuArea in Area.Storage)
			{
				foreach (var reset in dikuArea.MobileResets)
				{
					Mobile mobile;
					if (!allMobiles.TryGetValue(reset.MobileId, out mobile))
					{
						Console.WriteLine($"Warning: Unable to find mobile with Id {reset.MobileId}.");
						continue;
					}

					MMBRoom room;
					if (!allRooms.TryGetValue(reset.RoomId, out room))
					{
						Console.WriteLine($"Warning: Unable to find room with Id {reset.RoomId}.");
						continue;
					}

					if (room.Contents == null)
					{
						room.Contents = new List<MMBRoomContentRecord>();
					}

					var color = Color.Green;
					if (mobile.Flags.Contains(MobileFlags.Aggressive) && !mobile.Flags.Contains(MobileFlags.Wimpy))
					{
						color = Color.Red;
					}

					room.Contents.Add(new MMBRoomContentRecord($"{mobile.ShortDescription} #{mobile.Id}", color));
				}
			}

			// Save all areas
			var jsonFolder = Path.Combine(outputFolder, "maps/json");
			if (!Directory.Exists(jsonFolder))
			{
				Directory.CreateDirectory(jsonFolder);
			}

			var areasData = new List<AreaData>();
			foreach (var area in areas)
			{
				var fileName = $"{area.Name}.json";
				Log($"Saving {fileName}...");

				// Copy build options
				var options = new BuildOptions();

				if (area.Name == "Astoria")
				{
					options.RemoveSolitaryRooms = true;
					options.RemoveRoomsWithSingleOutsideExit = true;
				}

				var project = new MMBProject(area, options);
				var data = project.ToJson();
				File.WriteAllText(Path.Combine(jsonFolder, fileName), data);

				areasData.Add(new AreaData(area.Name, area.Credits, area.MinimumLevel, area.MaximumLevel));
			}

			var jsonOptions = CreateJsonOptions();
			var s = JsonSerializer.Serialize(areasData, jsonOptions);

			var dataFolder = DataFolder(outputFolder);
			var areasFile = Path.Combine(dataFolder, "areas.json");
			File.WriteAllText(areasFile, s);
		}

		static void ExportEquipment(string outputFolder)
		{
			var eqData = new List<EquipmentData>();
			foreach (var item in Item.Storage)
			{
				if (item.Flags.Contains(ItemFlags.HideFromEquipmentList) || item.ItemType != ItemType.Equipment)
				{
					continue;
				}

				var stats = string.Empty;
				if (item.EquipmentSlot != EquipmentSlotType.Wield)
				{
					Affect affectArmor;
					if (item.Affects.TryGetValue(ModifierType.Armor, out affectArmor))
					{
						stats = affectArmor.Value.ToString();
					}
					else
					{
						stats = "0";
					}
				}
				else
				{
					if (item.DamageRange != null)
					{
						stats = item.DamageRange.Value.ToString();
					}
				}

				var d = new EquipmentData(item.ShortDescription.SpaceToNbsp(),
					item.EquipmentSlot.ToString(),
					item.Material != null ? item.Material.ToString() : string.Empty,
					item.Price.ToString(),
					item.EnchantmentTier != null ? item.EnchantmentTier.Value.ToString() : string.Empty,
					stats);

				if (item.Flags.Contains(ItemFlags.Stab))
				{
					d.Affects.Add("Stab");
				}

				foreach (var pair in item.Affects)
				{
					if (item.EquipmentSlot != EquipmentSlotType.Wield &&
						pair.Key == ModifierType.Armor)
					{
						continue;
					}

					var affect = pair.Value;

					d.Affects.Add(affect.Type.ToPermanentAffect(affect.Value));
				}

				foreach (var forgeShop in ForgeShop.Storage)
				{
					foreach (var forge in forgeShop.Forges)
					{
						if (forge.Result.Id != item.Id)
						{
							continue;
						}

						if (forge.Price != 0)
						{
							d.ForgeComponents.Add($"{forge.Price} gold coins".SpaceToNbsp());
						}

						foreach (var component in forge.Components)
						{
							if (component.Quantity == 1)
							{
								d.ForgeComponents.Add(component.Name.SpaceToNbsp());
							}
							else
							{
								d.ForgeComponents.Add($"{component.Name} ({component.Quantity})".SpaceToNbsp());
							}
						}
					}
				}

				eqData.Add(d);
			}

			eqData = eqData.OrderBy(c => c.Slot).ThenBy(c => c.Name).ToList();

			var jsonOptions = CreateJsonOptions();
			var s = JsonSerializer.Serialize(eqData, jsonOptions);

			var dataFolder = DataFolder(outputFolder);
			var areasFile = Path.Combine(dataFolder, "equipment.json");
			File.WriteAllText(areasFile, s);
		}

		static void ExportConsumables(string outputFolder)
		{
			var consumableData = new List<ConsumableData>();
			foreach (var item in Item.Storage)
			{
				if (item.ItemType != ItemType.Potion && item.ItemType != ItemType.Scroll)
				{
					continue;
				}

				var d = new ConsumableData(item.ShortDescription,
					item.ItemType.ToString(),
					item.Price.ToString());

				foreach (var pair in item.Affects)
				{
					var affect = pair.Value;
					d.Affects.Add($"raises {affect.Type} by {affect.Value} for {affect.DurationInSeconds} seconds.");
				}

				consumableData.Add(d);
			}

			consumableData = consumableData.OrderBy(c => c.Type).ToList();

			var jsonOptions = CreateJsonOptions();
			var s = JsonSerializer.Serialize(consumableData, jsonOptions);

			var dataFolder = DataFolder(outputFolder);
			var constumableFile = Path.Combine(dataFolder, "consumables.json");
			File.WriteAllText(constumableFile, s);
		}

		static void ExportEnchantments(string outputFolder)
		{
			var enchantmentData = new List<EnchantmentData>();
			foreach (var enchantment in Enchantment.Storage)
			{
				var d = new EnchantmentData(enchantment.Name,
					enchantment.EnchantmentStones.ToString(),
					enchantment.SlotTypes.JoinOrAny(),
					enchantment.Materials.JoinOrAny(),
					enchantment.Price.ToString());

				foreach (var pair in enchantment.Affects)
				{
					d.Affects.Add(pair.Key.ToPermanentAffect(pair.Value));
				}

				enchantmentData.Add(d);
			}

			enchantmentData = enchantmentData.OrderBy(c => c.Name).ToList();

			var jsonOptions = CreateJsonOptions();
			var s = JsonSerializer.Serialize(enchantmentData, jsonOptions);

			var dataFolder = DataFolder(outputFolder);
			var constumableFile = Path.Combine(dataFolder, "enchantments.json");
			File.WriteAllText(constumableFile, s);
		}

		static void ExportSkill(StringBuilder sb, Skill skill)
		{
			sb.AppendLine($"#### {skill.Name.CasedName()}");
			sb.AppendLine();

			// Data
			for (var i = 0; i < skill.TotalLevels; ++i)
			{
				sb.Append($"{i + 1}|");

				var def = skill.Levels[i];

				var strings = new List<string>();

				if (def.Abilities != null)
				{
					foreach (var ab in def.Abilities)
					{
						strings.Add($"{ab.Name.CasedName()}");
					}
				}

				if (def.Modifiers != null)
				{
					foreach (var pair in def.Modifiers)
					{
						strings.Add(pair.Key.ToPermanentAffect(pair.Value));
					}
				}

				if (def.PrimeModifiers != null)
				{
					foreach (var pair in def.PrimeModifiers)
					{
						strings.Add($"{pair.Key.ToPermanentAffect(pair.Value)} (prime)");
					}
				}

				sb.Append(string.Join("<br>", strings.ToArray()));
				sb.AppendLine();
			}

			sb.AppendLine();
		}

		static void ExportSkills(StringBuilder sb, string className, string[] skillsOrder)
		{
			sb.AppendLine();
			sb.AppendLine($"### {className.CasedName()} Skills");
			sb.AppendLine();

			for (var i = 0; i < skillsOrder.Length; i++)
			{
				var skillName = skillsOrder[i];
				var skill = Skill.EnsureSkillById(skillName);

				ExportSkill(sb, skill);
			}
		}

		static void ExportSkills(string outputFolder)
		{
			var sb = new StringBuilder();

			sb.Append("---\r\nlayout: page\r\ntitle: Skills\r\n---");

			ExportSkills(sb, "warrior", new[] { "melee", "survival", "strength" });
			ExportSkills(sb, "rogue", new[] { "backstab", "dexterity" });
			ExportSkills(sb, "monk", new[] { "martialArts", "deathtouch", "constitution" });

			var skillsFile = Path.Combine(outputFolder, "skills.markdown");
			File.WriteAllText(skillsFile, sb.ToString());
		}

		static void Process(string inputFolder, string outputFolder)
		{
			StorageUtility.InitializeStorage(Log);

			DataContext.Load(inputFolder);

			ExportAreas(outputFolder);
			ExportEquipment(outputFolder);
			ExportConsumables(outputFolder);
			ExportEnchantments(outputFolder);
			ExportSkills(outputFolder);
		}

		static void Main(string[] args)
		{
			try
			{
				Process(@"D:\Projects\AbarimMUD\Data", @"D:\Projects\abarim-mud.github.io");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
