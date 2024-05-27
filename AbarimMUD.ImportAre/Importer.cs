using AbarimMUD.Data;
using AbarimMUD.Storage;
using GoRogue.DiceNotation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.ImportAre
{
	internal class Importer
	{
		private readonly Dictionary<int, Room> _roomsByVnums = new Dictionary<int, Room>();
		private readonly Dictionary<int, Mobile> _mobilesByVnums = new Dictionary<int, Mobile>();
		private readonly Dictionary<int, GameObject> _objectsByVnums = new Dictionary<int, GameObject>();

		private class RoomExitInfo
		{
			public Room SourceRoom { get; set; }
			public RoomExit RoomExit { get; set; }
			public int? TargetRoomVNum { get; set; }
			public int? KeyObjectVNum { get; set; }
		}

		private readonly List<RoomExitInfo> _tempDirections = new List<RoomExitInfo>();

		public static void Log(string message)
		{
			Console.WriteLine(message);
		}

		private Room GetRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		private Room EnsureRoomByVnum(int vnum) =>
			(from m in _roomsByVnums where m.Key == vnum select m.Value).First();

		private Mobile GetMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		private Mobile EnsureMobileByVnum(int vnum) =>
			(from m in _mobilesByVnums where m.Key == vnum select m.Value).First();

		private GameObject GetObjectByVnum(int vnum) =>
			(from m in _objectsByVnums where m.Key == vnum select m.Value).FirstOrDefault();

		private void ProcessArea(Stream stream, Area area)
		{
			var fileName = stream.ReadDikuString();
			area.Name = stream.ReadDikuString();
			area.Credits = stream.ReadDikuString();

			stream.ReadNumber(); // Start vnum
			stream.ReadNumber(); // End vnum

			Log($"Created area {area.Name}");
		}

		private void ProcessAreaData(Stream stream, Area area)
		{
			while (true)
			{
				var word = stream.EndOfStream() ? "End" : stream.ReadWord();

				switch (char.ToUpper(word[0]))
				{
					case 'C':
						area.Credits = stream.ReadDikuString();
						break;
					case 'N':
						area.Name = stream.ReadDikuString();
						break;
					case 'S':
						stream.ReadNumber(); // Security
						break;
					case 'V':
						if (word == "VNUMs")
						{
							stream.ReadNumber();
							stream.ReadNumber();
						}
						break;
					case 'E':
						if (word == "End")
						{
							goto finish;
						}
						break;
					case 'B':
						area.Builders = stream.ReadDikuString();
						break;
				}
			}

		finish:;
			Log($"Processed area data for {area.Name}");
		}

		private void ProcessMobiles(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString().Replace("oldstyle ", "");
				Log($"Processing mobile {name}...");

				var mobile = new Mobile
				{
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					LongDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
					Race = stream.ReadEnumFromDikuString<Race>(),
					Flags = (MobileFlags)stream.ReadFlag(),
					AffectedByFlags = (AffectedByFlags)stream.ReadFlag(),
//					Alignment = stream.ReadNumber(),
					Group = stream.ReadNumber(),
					Level = stream.ReadNumber(),
					HitRoll = stream.ReadNumber(),
					HitDice = stream.ReadDice(),
					ManaDice = stream.ReadDice(),
					DamageDice = stream.ReadDice(),
					AttackType = stream.ReadEnumFromWord<AttackType>(),
					AcPierce = stream.ReadNumber(),
					AcBash = stream.ReadNumber(),
					AcSlash = stream.ReadNumber(),
					AcExotic = stream.ReadNumber(),
					OffenseFlags = (MobileOffensiveFlags)stream.ReadFlag(),
					ImmuneFlags = (ResistanceFlags)stream.ReadFlag(),
					ResistanceFlags = (ResistanceFlags)stream.ReadFlag(),
					VulnerableFlags = (ResistanceFlags)stream.ReadFlag(),
					StartPosition = stream.ReadEnumFromWord<MobilePosition>(),
					Position = stream.ReadEnumFromWord<MobilePosition>(),
					Sex = stream.ReadEnumFromWord<Sex>(),
					Wealth = stream.ReadNumber(),
					FormsFlags = (FormFlags)stream.ReadFlag(),
					PartsFlags = (PartFlags)stream.ReadFlag(),
					Size = stream.ReadEnumFromWord<MobileSize>(),
					Material = stream.ReadEnumFromWord<Material>(),
				};

				var averageAc = (mobile.AcPierce + mobile.AcBash + mobile.AcSlash) / 3;
				mobile.ArmorClass = -(averageAc - 10) * 10;

				if (mobile.ArmorClass < 0)
				{
					stream.RaiseError($"Negative armor class");
				}

				var attacksCount = mobile.GetAttacksCount();
				var accuracy = mobile.GetAccuracy() + mobile.HitRoll * 10;
				for (var i = 0; i < attacksCount; ++i)
				{
					var expr = Dice.Parse(mobile.DamageDice);
					var min = expr.MinRoll();
					var max = expr.MaxRoll();

					var attack = new Attack(mobile.AttackType, accuracy, min, max);
					mobile.Attacks.Add(attack);
				}

				area.Mobiles.Add(mobile);
				_mobilesByVnums[vnum] = mobile;

				var raceInfo = mobile.Race.GetRaceInfo();

				mobile.Flags |= raceInfo.MobileFlags;
				mobile.AffectedByFlags |= raceInfo.AffectedByFlags;
				mobile.OffenseFlags |= raceInfo.OffensiveFlags;
				mobile.ImmuneFlags |= raceInfo.ImmuneFlags;
				mobile.ResistanceFlags |= raceInfo.ResistanceFlags;
				mobile.VulnerableFlags |= raceInfo.VulnerableFlags;
				mobile.FormsFlags |= raceInfo.FormFlags;
				mobile.PartsFlags |= raceInfo.PartFlags;


				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'F')
					{
						var word = stream.ReadWord();
						var vector = stream.ReadFlag();

						switch (word.Substring(0, 3).ToLower())
						{
							case "act":
								mobile.Flags &= (MobileFlags)(~vector);
								break;
							case "aff":
								mobile.AffectedByFlags &= (AffectedByFlags)(~vector);
								break;
							case "off":
								mobile.OffenseFlags &= (MobileOffensiveFlags)(~vector);
								break;
							case "imm":
								mobile.ImmuneFlags &= (ResistanceFlags)(~vector);
								break;
							case "res":
								mobile.ResistanceFlags &= (ResistanceFlags)(~vector);
								break;
							case "vul":
								mobile.VulnerableFlags &= (ResistanceFlags)(~vector);
								break;
							case "for":
								mobile.FormsFlags &= (FormFlags)(~vector);
								break;
							case "par":
								mobile.PartsFlags &= (PartFlags)(~vector);
								break;
							default:
								stream.RaiseError($"Unknown flag {word}");
								break;
						}
					}
					else if (c == 'M')
					{
						var word = stream.ReadWord();
						var mnum = stream.ReadNumber();
						var trig = stream.ReadDikuString();

						Log("Warning: mob triggers are ignored.");
					}
					else
					{
						stream.GoBackIfNotEOF();
						break;
					}
				}
			}
		}

		private void ProcessObjects(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString();
				Log($"Processing object {name}...");

				var obj = new GameObject
				{
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
					Material = stream.ReadEnumFromDikuStringWithDef(Material.Generic),
					ItemType = stream.ReadEnumFromWord<ItemType>(),
					ExtraFlags = (ItemExtraFlags)stream.ReadFlag(),
					WearFlags = (ItemWearFlags)stream.ReadFlag(),
				};

				area.Objects.Add(obj);
				_objectsByVnums[vnum] = obj;

				switch (obj.ItemType)
				{
					case ItemType.Weapon:
						obj.Value1 = (int)stream.ReadEnumFromWord<WeaponType>();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = stream.ReadNumber();
						obj.Value4 = (int)stream.ReadEnumFromWord<AttackType>();
						obj.Value5 = stream.ReadFlag();
						break;
					case ItemType.Container:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadFlag();
						obj.Value3 = stream.ReadNumber();
						obj.Value4 = stream.ReadNumber();
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Drink:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = (int)stream.ReadEnumFromWord<LiquidType>();
						obj.Value4 = stream.ReadNumber();
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Fountain:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = (int)stream.ReadEnumFromWord<LiquidType>();
						obj.Value4 = stream.ReadNumber();
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Wand:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = stream.ReadNumber();
						obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Staff:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = stream.ReadNumber();
						obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Potion:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						break;
					case ItemType.Pill:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						break;
					case ItemType.Scroll:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value3 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value4 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						obj.Value5 = (int)stream.ReadEnumFromWordWithDef(Skill.Reserved);
						break;
					default:
						obj.Value1 = stream.ReadFlag();
						obj.Value2 = stream.ReadFlag();
						obj.Value3 = stream.ReadFlag();
						obj.Value4 = stream.ReadFlag();
						obj.Value5 = stream.ReadFlag();
						break;
				}

				obj.Level = stream.ReadNumber();
				obj.Weight = stream.ReadNumber();
				obj.Cost = stream.ReadNumber();

				var letter = stream.ReadSpacedLetter();

				switch (letter)
				{
					case 'P':
						obj.Condition = 100;
						break;
					case 'G':
						obj.Condition = 90;
						break;
					case 'A':
						obj.Condition = 75;
						break;
					case 'W':
						obj.Condition = 50;
						break;
					case 'D':
						obj.Condition = 25;
						break;
					case 'B':
						obj.Condition = 10;
						break;
					case 'R':
						obj.Condition = 0;
						break;
					default:
						obj.Condition = 100;
						break;
				}

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'A')
					{
						var effect = new GameObjectEffect
						{
							EffectBitType = EffectBitType.Object,
							EffectType = (EffectType)stream.ReadNumber(),
							Modifier = stream.ReadNumber()
						};

						obj.Effects.Add(effect);
					}
					else if (c == 'F')
					{
						var effect = new GameObjectEffect();
						c = stream.ReadSpacedLetter();
						switch (c)
						{
							case 'A':
								effect.EffectBitType = EffectBitType.None;
								break;
							case 'I':
								effect.EffectBitType = EffectBitType.Immunity;
								break;
							case 'R':
								effect.EffectBitType = EffectBitType.Resistance;
								break;
							case 'V':
								effect.EffectBitType = EffectBitType.Vulnerability;
								break;
							default:
								stream.RaiseError($"Unable to parse effect bit '{c}'");
								break;
						}

						effect.EffectType = (EffectType)stream.ReadNumber();
						effect.Modifier = stream.ReadNumber();
						effect.Bits = (AffectedByFlags)stream.ReadFlag();
						obj.Effects.Add(effect);
					}
					else if (c == 'E')
					{
						obj.ExtraKeyword = stream.ReadDikuString();
						obj.ExtraDescription = stream.ReadDikuString();
					}
					else
					{
						stream.GoBackIfNotEOF();
						break;
					}
				}
			}
		}

		private void ProcessRooms(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var vnum = int.Parse(stream.ReadId());
				if (vnum == 0)
				{
					break;
				}

				var name = stream.ReadDikuString();
				Log($"Processing room {name}...");

				var room = new Room
				{
					Name = name,
					Description = stream.ReadDikuString(),
				};

				stream.ReadNumber(); // Area Number

				room.Flags = (RoomFlags)stream.ReadFlag();
				room.SectorType = stream.ReadEnumFromWord<SectorType>();

				// Save room to set its id
				area.Rooms.Add(room);
				_roomsByVnums[vnum] = room;

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'S')
					{
						break;
					}
					else if (c == 'H')
					{
						room.HealRate = stream.ReadNumber();
					}
					else if (c == 'M')
					{
						room.ManaRate = stream.ReadNumber();
					}
					else if (c == 'C')
					{
						string clan = stream.ReadDikuString();
					}
					else if (c == 'D')
					{
						var exit = new RoomExit
						{
							Direction = (Direction)stream.ReadNumber(),
							Description = stream.ReadDikuString(),
							Keyword = stream.ReadDikuString(),
						};

						var locks = stream.ReadNumber();
						switch (locks)
						{
							case 1:
								exit.Flags = RoomExitFlags.Door;
								break;
							case 2:
								exit.Flags = RoomExitFlags.Door | RoomExitFlags.PickProof;
								break;
							case 3:
								exit.Flags = RoomExitFlags.Door | RoomExitFlags.NoPass;
								break;
							case 4:
								exit.Flags = RoomExitFlags.Door | RoomExitFlags.PickProof | RoomExitFlags.NoPass;
								break;
							default:
								break;
						}

						var exitInfo = new RoomExitInfo
						{
							SourceRoom = room,
							RoomExit = exit
						};

						var keyVNum = stream.ReadNumber();
						if (exit.Flags != RoomExitFlags.None && keyVNum != -1)
						{
							exitInfo.KeyObjectVNum = keyVNum;
						}

						var targetVnum = stream.ReadNumber();
						if (targetVnum != -1)
						{
							exitInfo.TargetRoomVNum = targetVnum;
						}

						_tempDirections.Add(exitInfo);
					}
					else if (c == 'E')
					{
						room.ExtraKeyword = stream.ReadDikuString();
						room.ExtraDescription = stream.ReadDikuString();
					}
					else if (c == 'O')
					{
						room.Owner = stream.ReadDikuString();
					}
					else
					{
						throw new Exception($"Unknown room command '{c}'");
					}
				}
			}
		}

		private void ProcessResets(Stream stream, Area area)
		{
			while (!stream.EndOfStream())
			{
				var c = stream.ReadSpacedLetter();
				if (c == 'S')
				{
					break;
				}

				if (c == '*')
				{
					stream.ReadLine();
					continue;
				}

				var reset = new AreaReset();
				switch (c)
				{
					case 'M':
						reset.ResetType = AreaResetType.NPC;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						reset.Value5 = stream.ReadNumber();
						break;

					case 'O':
						reset.ResetType = AreaResetType.Item;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						break;

					case 'P':
						reset.ResetType = AreaResetType.Put;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						reset.Value5 = stream.ReadNumber();
						break;

					case 'G':
						reset.ResetType = AreaResetType.Give;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						break;

					case 'E':
						reset.ResetType = AreaResetType.Equip;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						break;

					case 'D':
						reset.ResetType = AreaResetType.Door;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();

						break;

					case 'R':
						reset.ResetType = AreaResetType.Randomize;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();

						break;
				}

				stream.ReadLine();
				area.Resets.Add(reset);
			}
		}

		private void ProcessShops(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var keeperVnum = stream.ReadNumber();
				if (keeperVnum == 0)
				{
					break;
				}

				var keeper = GetMobileByVnum(keeperVnum);
				if (keeper == null)
				{
					stream.RaiseError($"Could not find shop keeper with vnum {keeperVnum}");
				}

				var shop = new Shop
				{
					BuyType1 = stream.ReadNumber(),
					BuyType2 = stream.ReadNumber(),
					BuyType3 = stream.ReadNumber(),
					BuyType4 = stream.ReadNumber(),
					BuyType5 = stream.ReadNumber(),
					ProfitBuy = stream.ReadNumber(),
					ProfitSell = stream.ReadNumber(),
					OpenHour = stream.ReadNumber(),
					CloseHour = stream.ReadNumber(),
				};

				keeper.Shop = shop;

				stream.ReadLine();

				Log($"Added shop for mobile {keeper.Name}");
			}
		}

		private void ProcessSpecials(Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var c = stream.ReadSpacedLetter();
				switch (c)
				{
					case 'S':
						return;

					case '*':
						break;

					case 'M':
						var mobVnum = stream.ReadNumber();
						var mobile = GetMobileByVnum(mobVnum);
						if (mobile == null)
						{
							throw new Exception($"Could not find mobile with vnum {mobVnum}");
						}

						var special = new MobileSpecialAttack
						{
							AttackType = stream.ReadWord()
						};

						mobile.SpecialAttacks.Add(special);
						break;
				}

				stream.ReadLine();
			}
		}

		private void ProcessSocials(DataContext db, Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var temp = stream.ReadWord();
				if (temp == "#0")
				{
					break;
				}

				var social = new Social
				{
					Name = temp,
				};

				Log($"Processing social {temp}...");

				do
				{
					string s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharNoArg = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersNoArg = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.VictFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharNotFound = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.CharAuto = s;

					s = string.Empty;
					if (!stream.ReadSocialString(ref s))
					{
						break;
					}
					social.OthersAuto = s;
				}
				while (false);

				db.Socials.Add(social);
			}

			db.Socials.Save();
		}

		private void ProcessFile(DataContext db, string areaFile)
		{
			Log($"Processing {areaFile}...");
			Area area = null;
			using (var stream = File.OpenRead(areaFile))
			{
				while (!stream.EndOfStream())
				{
					var type = stream.ReadId();
					switch (type)
					{
						case "AREA":
							area = new Area();
							ProcessArea(stream, area);
							break;
						case "AREADATA":
							area = new Area();
							ProcessAreaData(stream, area);
							break;
						case "MOBILES":
							ProcessMobiles(stream, area);
							break;
						case "OBJECTS":
							ProcessObjects(stream, area);
							break;
						case "ROOMS":
							ProcessRooms(stream, area);
							break;
						case "MOBOLD":
							stream.RaiseError("Old mobiles aren't supported");
							break;
						case "MOBPROGS":
							Log("Mob programs aren't supported");
							break;
						case "OBJOLD":
							stream.RaiseError("Old objects aren't supported");
							break;
						case "RESETS":
							ProcessResets(stream, area);
							break;
						case "SHOPS":
							ProcessShops(stream);
							break;
						case "SPECIALS":
							ProcessSpecials(stream);
							break;
						case "HELPS":
							// Ignore helps
							goto finish;
						case "SOCIALS":
							ProcessSocials(db, stream);
							goto finish;
						case "$":
							if (area != null)
							{
								db.Areas.Update(area);
							}
							goto finish;
						default:
							stream.RaiseError($"Sections {type} aren't supported.");
							break;
					}
				}

			finish:;
			}
		}

		private static void SetIds<T>(Dictionary<int, T> data) where T: AreaEntity
		{
			var id = 1;
			foreach(var pair in data)
			{
				pair.Value.Id = id;
				++id;
			}
		}

		public void Process()
		{
			var inputDir = Path.Combine(Utility.ExecutingAssemblyDirectory, "../../../../SourceContent");
			var areaFiles = Directory.EnumerateFiles(inputDir, "*.are", SearchOption.AllDirectories).ToArray();

			var outputDir = Path.Combine(Utility.ExecutingAssemblyDirectory, "../../../../Data");
			var areasFolder = Path.Combine(outputDir, "areas");
			if (Directory.Exists(areasFolder))
			{
				Directory.Delete(areasFolder, true);
			}

			var db = new DataContext(outputDir, Log);
			foreach (var areaFile in areaFiles)
			{
				if (Path.GetFileName(areaFile) == "proto.are")
				{
					Log($"Skipping prototype area {areaFile}");
					continue;
				}

				ProcessFile(db, areaFile);
			}

			// Set ids
			SetIds(_roomsByVnums);
			SetIds(_mobilesByVnums);
			SetIds(_objectsByVnums);

			// Process directions
			Log("Updating directions");
			foreach (var dir in _tempDirections)
			{
				var exit = dir.RoomExit;
				if (dir.TargetRoomVNum != null)
				{
					exit.TargetRoom = EnsureRoomByVnum(dir.TargetRoomVNum.Value);
				}

				if (dir.KeyObjectVNum != null)
				{
					var keyObj = GetObjectByVnum(dir.KeyObjectVNum.Value);
					if (keyObj != null)
					{
						exit.KeyObjectId = keyObj.Id;
					}
				}

				dir.SourceRoom.Exits[exit.Direction] = exit;
			}

			// Update resets
			foreach (var area in db.Areas)
			{
				foreach(var reset in area.Resets)
				{
					switch (reset.ResetType)
					{
						case AreaResetType.NPC:
							reset.Value2 = EnsureMobileByVnum(reset.Value2).Id;
							reset.Value4 = EnsureRoomByVnum(reset.Value4).Id;
							break;
						case AreaResetType.Item:
							break;
						case AreaResetType.Put:
							break;
						case AreaResetType.Give:
							break;
						case AreaResetType.Equip:
							break;
						case AreaResetType.Door:
							break;
						case AreaResetType.Randomize:
							break;
					}
				}
			}

			// Update all areas
			foreach (var area in db.Areas)
			{
				db.Areas.Update(area);
			}

			/*						Log("Locking doors");
									using (var db = new DataContext())
									{
										var areas = (from a in db.Areas select a).ToArray();
										foreach (var area in areas)
										{
											var resets = (from r in db.AreaResets where r.Area == area select r).ToArray();
											foreach (var reset in resets)
											{
												if (reset.ResetType != AreaResetType.Door)
												{
													continue;
												}

												if (reset.Value3 < 0 || reset.Value3 >= 6)
												{
													throw new Exception($"Reset {reset.Id}. Room direction with value {reset.Value3} is outside of range.");
												}

												var dir = (Direction)reset.Value3;

												var roomVnum = reset.Value2;

												var room = (from r in db.Rooms where r.Id == roomVnum select r).FirstOrDefault();
												if (room == null)
												{
													throw new Exception($"Reset {reset.Id}. Can't find room with vnum {roomVnum}");
												}

												var exit = (from e in db.RoomsExits where e.SourceRoomId == room.Id && 
															e.Direction == (Direction)reset.Value3 
															select e).FirstOrDefault();
												if (exit == null || !exit.Flags.HasFlag(RoomExitFlags.Door))
												{
													throw new Exception($"Reset {reset.Id}. Can't find exit {dir}");
												}

												switch (reset.Value4)
												{
													case 0:
														break;
													case 1:
														exit.Flags |= RoomExitFlags.Closed;
														db.SaveChanges();
														break;
													case 2:
														exit.Flags |= RoomExitFlags.Closed | RoomExitFlags.Locked;
														db.SaveChanges();
														break;
													default:
														throw new Exception($"Reset {reset.Id}. Bad locks {reset.Value4}");

												}
											}
										}
									}*/

			Log("Success");
		}
	}
}