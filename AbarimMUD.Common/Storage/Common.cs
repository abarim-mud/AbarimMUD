using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class SkillValueConverterType : JsonConverter<SkillValue>
		{
			public override SkillValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				// It should contain only skill level
				// The id is key in the dictionary
				var value = reader.GetString();

				var level = int.Parse(value);

				return new SkillValue
				{
					Level = level
				};
			}

			public override void Write(Utf8JsonWriter writer, SkillValue value, JsonSerializerOptions options)
			{
				// Write only level
				// The skill id is key of the dict
				writer.WriteStringValue(value.Level.ToString());
			}
		}

		public class AffectsConverterType : JsonConverter<Dictionary<ModifierType, Affect>>
		{
			public override Dictionary<ModifierType, Affect> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var result = new Dictionary<ModifierType, Affect>();
				var doc = JsonDocument.ParseValue(ref reader);

				foreach (var pair in doc.RootElement.EnumerateObject())
				{
					var modifier = Enum.Parse<ModifierType>(pair.Name);

					if (pair.Value.ValueKind == JsonValueKind.Number)
					{
						// No duration
						result[modifier] = new Affect(modifier, pair.Value.GetInt32());
					}
					else
					{
						// Duration
						result[modifier] = JsonSerializer.Deserialize<Affect>(pair.Value, options);
						result[modifier].Type = modifier;
					}
				}

				return result;
			}

			public override void Write(Utf8JsonWriter writer, Dictionary<ModifierType, Affect> value, JsonSerializerOptions options)
			{
				var newDict = new Dictionary<ModifierType, object>();

				foreach (var pair in value)
				{
					// If there's no duration, then write just value
					if (pair.Value.DurationInSeconds == null)
					{
						newDict[pair.Key] = pair.Value.Value;
					}
					else
					{
						newDict[pair.Key] = pair.Value;
					}
				}

				JsonSerializer.Serialize(writer, newDict, options);
			}
		}

		public class AbilitiesConverterType : JsonConverter<Dictionary<string, AbilityPower>>
		{
			public override Dictionary<string, AbilityPower> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var result = new Dictionary<string, AbilityPower>();
				var doc = JsonDocument.ParseValue(ref reader);

				foreach (var pair in doc.RootElement.EnumerateObject())
				{
					var ap = new AbilityPower
					{
						Ability = new Ability
						{
							Id = pair.Name
						},
						Power = int.Parse(pair.Value.ToString())
					};

					result[pair.Name] = ap;
				}

				return result;
			}

			public override void Write(Utf8JsonWriter writer, Dictionary<string, AbilityPower> value, JsonSerializerOptions options)
			{
				var newDict = new Dictionary<string, int>();

				foreach (var pair in value)
				{
					newDict[pair.Key] = pair.Value.Power;
				}

				JsonSerializer.Serialize(writer, newDict, options);
			}
		}

		public class ItemInstanceConverterType : JsonConverter<ItemInstance>
		{
			public override ItemInstance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					// Just an id
					var itemId = reader.GetString();

					return new ItemInstance
					{
						Info = new Item { Id = itemId }
					};
				}

				// Standard parse
				var doc = JsonDocument.ParseValue(ref reader);

				// We need to ignore the converter to avoid infinite recursion
				return JsonSerializer.Deserialize<ItemInstance>(doc, options.CreateCopyWithout(ItemInstanceConverter));
			}

			public override void Write(Utf8JsonWriter writer, ItemInstance value, JsonSerializerOptions options)
			{
				// Write just an id
				if (value.Enchantment == null)
				{
					writer.WriteStringValue(value.Id);
					return;
				}

				// Full serialization
				// We need to ignore the converter to avoid infinite recursion
				JsonSerializer.Serialize(writer, value, options.CreateCopyWithout(ItemInstanceConverter));
			}
		}

		public class InventoryConverterType : JsonConverter<Inventory>
		{
			public override Inventory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var doc = JsonDocument.ParseValue(ref reader);

				return new Inventory
				{
					Items = JsonSerializer.Deserialize<List<InventoryRecord>>(doc, options)
				};
			}

			public override void Write(Utf8JsonWriter writer, Inventory value, JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value.Items, options);
			}
		}

		public class EquipmentConverterType : JsonConverter<Equipment>
		{
			public override Equipment Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var doc = JsonDocument.ParseValue(ref reader);

				var dict = JsonSerializer.Deserialize<Dictionary<string, ItemInstance>>(doc, options);

				var result = new Equipment();
				foreach (var pair in dict)
				{
					var parts = pair.Key.Split(':', StringSplitOptions.TrimEntries);
					var slotType = Enum.Parse<EquipmentSlotType>(parts[0]);
					var index = 0;

					if (parts.Length > 1)
					{
						index = int.Parse(parts[1]);
					}

					var slot = result.GetSlot(slotType, index);
					if (slot == null)
					{
						// TODO: Log warning of non-existance slot
					}
					else
					{
						slot.Item = pair.Value;
					}
				}

				return result;
			}

			public override void Write(Utf8JsonWriter writer, Equipment value, JsonSerializerOptions options)
			{
				// Write just an id
				var dict = new Dictionary<string, ItemInstance>();
				foreach (var slot in value.Slots)
				{
					if (slot.Item == null)
					{
						continue;
					}

					var key = slot.SlotType.ToString();

					if (slot.Index > 0)
					{
						key += $":{slot.Index}";
					}

					dict[key] = slot.Item;
				}


				JsonSerializer.Serialize(writer, dict, options);
			}
		}

		public class MobileSpawnConverterType : JsonConverter<MobileSpawn>
		{
			public override MobileSpawn Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					// Just an id
					var mobileId = reader.GetString();

					return new MobileSpawn
					{
						Mobile = new Mobile
						{
							Id = int.Parse(mobileId)
						}
					};
				}

				// Standard parse
				var doc = JsonDocument.ParseValue(ref reader);

				// We need to ignore the converter to avoid infinite recursion
				return JsonSerializer.Deserialize<MobileSpawn>(doc, options.CreateCopyWithout(MobileSpawnConverter));
			}

			public override void Write(Utf8JsonWriter writer, MobileSpawn value, JsonSerializerOptions options)
			{
				// Write just an id
				if (!value.HasCustomParams)
				{
					writer.WriteStringValue(value.Mobile.Id.ToString());
					return;
				}

				// Full serialization
				// We need to ignore the converter to avoid infinite recursion
				JsonSerializer.Serialize(writer, value, options.CreateCopyWithout(MobileSpawnConverter));
			}
		}

		public class InstantEffectConverterType : JsonConverter<InstantEffect>
		{
			public override InstantEffect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var s = reader.GetString();

				var parts = s.Split(':', StringSplitOptions.TrimEntries);

				var type = Enum.Parse<InstantEffectType>(parts[0]);
				var power = ValueRange.Parse(parts[1]);

				return new InstantEffect
				{
					Type = type,
					Power = power
				};
			}

			public override void Write(Utf8JsonWriter writer, InstantEffect value, JsonSerializerOptions options)
			{
				var s = $"{value.Type}:{value.Power}";
				writer.WriteStringValue(s);
			}
		}

		public class RoomExitConverterType : JsonConverter<RoomExit>
		{
			public override RoomExit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var id = reader.GetInt32();
				return new RoomExit
				{
					TargetRoom = new Room
					{
						Id = id
					}
				};
			}

			public override void Write(Utf8JsonWriter writer, RoomExit value, JsonSerializerOptions options)
			{
				writer.WriteNumberValue(value.TargetRoom.Id);
			}
		}

		public static readonly RoomExitConverterType RoomExitConverter = new RoomExitConverterType();
		public static readonly ItemInstanceConverterType ItemInstanceConverter = new ItemInstanceConverterType();
		public static readonly SkillValueConverterType SkillValueConverter = new SkillValueConverterType();
		public static readonly AffectsConverterType AffectsConverter = new AffectsConverterType();
		public static readonly AbilitiesConverterType AbilitiesConverter = new AbilitiesConverterType();
		public static readonly InventoryConverterType InventoryConverter = new InventoryConverterType();
		public static readonly EquipmentConverterType EquipmentConverter = new EquipmentConverterType();
		public static readonly MobileSpawnConverterType MobileSpawnConverter = new MobileSpawnConverterType();
		public static readonly InstantEffectConverterType InstantEffectConverter = new InstantEffectConverterType();

		public static void SetReferences(this ItemInstance item)
		{
			item.Info = Item.EnsureItemById(item.Info.Id);

			if (item.Enchantment != null)
			{
				item.Enchantment = Enchantment.GetEnchantmentById(item.Enchantment.Id);
			}
		}

		public static void SetReferences(this InventoryRecord rec)
		{
			rec.Item.SetReferences();
		}

		public static void SetReferences(this EquipmentSlot slot)
		{
			if (slot.Item == null)
			{
				return;
			}

			slot.Item.SetReferences();
		}

		public static void SetReferences(this IEnumerable<ItemInstance> inv)
		{
			foreach (var rec in inv)
			{
				rec.SetReferences();
			}
		}

		public static void SetReferences(this IEnumerable<InventoryRecord> inv)
		{
			foreach (var rec in inv)
			{
				rec.SetReferences();
			}
		}

		public static void SetReferences(this Equipment eq)
		{
			foreach (var slot in eq.Slots)
			{
				slot.SetReferences();
			}
		}

		public static void SetReferences(this Mobile mobile)
		{
			if (mobile.Class != null)
			{
				mobile.Class = MobileClass.EnsureClassById(mobile.Class.Id);
			} else
			{
				mobile.Class = MobileClass.EnsureClassById(Configuration.DefaultMobileClassId);
			}

			if (mobile.Loot != null)
			{
				foreach (var loot in mobile.Loot)
				{
					loot.Items.SetReferences();
				}
			}

			if (mobile.Shop != null)
			{
				mobile.Shop = Shop.EnsureShopById(mobile.Shop.Id);
			}

			if (mobile.ForgeShop != null)
			{
				mobile.ForgeShop = ForgeShop.EnsureForgeShopById(mobile.ForgeShop.Id);
			}

			if (mobile.ExchangeShop != null)
			{
				mobile.ExchangeShop = ExchangeShop.EnsureExchangeShopById(mobile.ExchangeShop.Id);
			}

			if (mobile.Guildmaster != null)
			{
				mobile.Guildmaster = PlayerClass.EnsureClassById(mobile.Guildmaster.Id);
			}

			mobile.Experience = mobile.CalculateXpAward();
		}
	}
}
