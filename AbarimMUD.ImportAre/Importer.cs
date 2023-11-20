using AbarimMUD.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.ImportAre
{
	internal class Importer
	{
		private readonly List<RoomDirection> _tempDirections = new List<RoomDirection>();

		public static void Log(string message)
		{
			Console.WriteLine(message);
		}

		private void ProcessArea(DataContext db, Stream stream, Area area)
		{
			var fileName = stream.ReadDikuString();
			area.Name = stream.ReadDikuString();
			area.Credits = stream.ReadDikuString();

			stream.ReadNumber(); // Start vnum
			stream.ReadNumber(); // End vnum

			db.Areas.Add(area);
			db.SaveChanges();

			Log($"Created area {area.Name}");
		}

		private void ProcessAreaData(DataContext db, Stream stream, Area area)
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
			db.Areas.Add(area);
			db.SaveChanges();

			Log($"Created area {area.Name}");
		}

		private void ProcessMobiles(DataContext db, Stream stream, Area area)
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
					AreaId = area.Id,
					VNum = vnum,
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					LongDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
					Race = stream.ReadEnumFromDikuString<Race>(),
					MobileFlags = (MobileFlags)stream.ReadFlag(),
					AffectedByFlags = (AffectedByFlags)stream.ReadFlag(),
					Alignment = stream.ReadNumber(),
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
					DefaultPosition = stream.ReadEnumFromWord<MobilePosition>(),
					Sex = stream.ReadEnumFromWord<Sex>(),
					Wealth = stream.ReadNumber(),
					FormsFlags = (FormFlags)stream.ReadFlag(),
					PartsFlags = (PartFlags)stream.ReadFlag(),
					Size = stream.ReadEnumFromWord<MobileSize>(),
					Material = stream.ReadEnumFromWord<Material>(),
				};

				var raceInfo = mobile.Race.GetRaceInfo();

				mobile.MobileFlags |= raceInfo.MobileFlags;
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
								mobile.MobileFlags &= (MobileFlags)(~vector);
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

				db.Mobiles.Add(mobile);
				db.SaveChanges();
			}
		}

		private void ProcessObjects(DataContext db, Stream stream, Area area)
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
					AreaId = area.Id,
					VNum = vnum,
					Name = name,
					ShortDescription = stream.ReadDikuString(),
					Description = stream.ReadDikuString(),
					Material = stream.ReadEnumFromDikuStringWithDef(Material.Generic),
					ItemType = stream.ReadEnumFromWord<ItemType>(),
					ExtraFlags = (ItemExtraFlags)stream.ReadFlag(),
					WearFlags = (ItemWearFlags)stream.ReadFlag(),
				};

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

				db.Objects.Add(obj);
				db.SaveChanges();

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'A')
					{
						var effect = new GameObjectEffect
						{
							GameObjectId = obj.Id,
							EffectBitType = EffectBitType.Object,
							EffectType = (EffectType)stream.ReadNumber(),
							Modifier = stream.ReadNumber()
						};

						db.ObjectsEffect.Add(effect);
					}
					else if (c == 'F')
					{
						var effect = new GameObjectEffect
						{
							GameObjectId = obj.Id
						};

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

						db.ObjectsEffect.Add(effect);
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

				db.SaveChanges();
			}
		}

		private void ProcessRooms(DataContext db, Stream stream, Area area)
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
					AreaId = area.Id,
					VNum = vnum,
					Name = name,
					Description = stream.ReadDikuString(),
				};

				stream.ReadNumber(); // Area Number

				room.Flags = (RoomFlags)stream.ReadFlag();
				room.SectorType = stream.ReadEnumFromWord<SectorType>();

				// Save room to set its id
				db.Rooms.Add(room);
				db.SaveChanges();

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
						var roomDirection = new RoomDirection
						{
							SourceRoomId = room.Id,
							DirectionType = (DirectionType)stream.ReadNumber(),
							Description = stream.ReadDikuString(),
							Keyword = stream.ReadDikuString(),
						};

						var locks = stream.ReadNumber();
						switch (locks)
						{
							case 1:
								roomDirection.Flags = RoomDirectionFlags.Door;
								break;
							case 2:
								roomDirection.Flags = RoomDirectionFlags.Door | RoomDirectionFlags.PickProof;
								break;
							case 3:
								roomDirection.Flags = RoomDirectionFlags.Door | RoomDirectionFlags.NoPass;
								break;
							case 4:
								roomDirection.Flags = RoomDirectionFlags.Door | RoomDirectionFlags.PickProof | RoomDirectionFlags.NoPass;
								break;
							default:
								break;
						}

						var keyVNum = stream.ReadNumber();
						if (roomDirection.Flags != RoomDirectionFlags.None && keyVNum != -1)
						{
							roomDirection.KeyObjectVNum = keyVNum;
						}

						var targetVnum = stream.ReadNumber();
						if (targetVnum != -1)
						{
							roomDirection.TargetRoomVNum = targetVnum;
						}

						_tempDirections.Add(roomDirection);
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

				db.SaveChanges();
			}
		}

		private void ProcessResets(DataContext db, Stream stream, Area area)
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

				var reset = new AreaReset
				{
					AreaId = area.Id
				};

				switch (c)
				{
					case 'M':
						reset.ResetType = AreaResetType.Mobile;
						reset.Value1 = stream.ReadNumber();
						reset.Value2 = stream.ReadNumber();
						reset.Value3 = stream.ReadNumber();
						reset.Value4 = stream.ReadNumber();
						reset.Value5 = stream.ReadNumber();
						break;

					case 'O':
						reset.ResetType = AreaResetType.GameObject;
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

				db.AreaResets.Add(reset);
				db.SaveChanges();
			}
		}

		private void ProcessShops(DataContext db, Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var keeperVnum = stream.ReadNumber();
				if (keeperVnum == 0)
				{
					break;
				}

				var keeper = (from m in db.Mobiles where m.VNum == keeperVnum select m).FirstOrDefault();
				if (keeper == null)
				{
					stream.RaiseError($"Could not find shop keeper with vnum {keeperVnum}");
				}

				var shop = new Shop
				{
					MobileId = keeper.Id,
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

				stream.ReadLine();

				db.Shops.Add(shop);
				db.SaveChanges();

				Log($"Added shop for mobile {keeper.Name}");
			}
		}

		private void ProcessSpecials(DataContext db, Stream stream)
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
						var mobile = (from m in db.Mobiles where m.VNum == mobVnum select m).FirstOrDefault();
						if (mobile == null)
						{
							throw new Exception($"Could not find mobile with vnum {mobVnum}");
						}

						var special = new MobileSpecialAttack
						{
							MobileId = mobile.Id,
							AttackType = stream.ReadWord()
						};

						db.MobileSpecialAttacks.Add(special);
						db.SaveChanges();

						break;
				}

				stream.ReadLine();
			}
		}

		private void ProcessHelps(DataContext db, Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var level = stream.ReadNumber();
				var keyword = stream.ReadDikuString();

				if (keyword[0] == '$')
				{
					break;
				}

				var helpData = new HelpData
				{
					Level = level,
					Keyword = keyword,
					Text = stream.ReadDikuString()
				};

				db.Helps.Add(helpData);
				db.SaveChanges();
			}
		}

		private void ProcessSocials(DataContext db, Stream stream)
		{
			while (!stream.EndOfStream())
			{
				var temp = stream.ReadWord();
				if (temp == "#0")
				{
					return;
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
				db.SaveChanges();
			}
		}

		private void ProcessFile(string areaFile)
		{
			Log($"Processing {areaFile}...");
			using (var db = new DataContext())
			{
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
								ProcessArea(db, stream, area);
								break;
							case "AREADATA":
								area = new Area();
								ProcessAreaData(db, stream, area);
								break;
							case "MOBILES":
								ProcessMobiles(db, stream, area);
								break;
							case "OBJECTS":
								ProcessObjects(db, stream, area);
								break;
							case "ROOMS":
								ProcessRooms(db, stream, area);
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
								ProcessResets(db, stream, area);
								break;
							case "SHOPS":
								ProcessShops(db, stream);
								break;
							case "SPECIALS":
								ProcessSpecials(db, stream);
								break;
							case "HELPS":
								ProcessHelps(db, stream);
								break;
							case "SOCIALS":
								ProcessSocials(db, stream);
								break;
							case "$":
								goto finish;
							default:
								stream.RaiseError($"Sections {type} aren't supported.");
								break;
						}
					}

				finish:;
				}
			}
		}

		public void Process()
		{
			var InputDir = Path.Combine(Utility.ExecutingAssemblyDirectory, "../../../../SourceContent");
			var areaFiles = Directory.EnumerateFiles(InputDir, "*.are", SearchOption.AllDirectories).ToArray();

			// Recreate the db
			using (var db = new DataContext())
			{
				db.Database.EnsureDeleted();
				db.Database.EnsureCreated();
			}

			foreach (var areaFile in areaFiles)
			{
				if (Path.GetFileName(areaFile) == "proto.are")
				{
					Log($"Skipping prototype area {areaFile}");
					continue;
				}

				ProcessFile(areaFile);
			}

			// Process directions
			Log("Updating directions");
			foreach (var dir in _tempDirections)
			{
				using (var db = new DataContext())
				{
					if (dir.TargetRoomVNum != null)
					{
						dir.TargetRoomId = (from r in db.Rooms where r.VNum == dir.TargetRoomVNum.Value select r).First().Id;
					}

					if (dir.KeyObjectVNum != null)
					{
						var keyObj = (from o in db.Objects where o.VNum == dir.KeyObjectVNum.Value select o).FirstOrDefault();
						if (keyObj != null)
						{
							dir.KeyObjectId = keyObj.Id;
						}
					}

					db.RoomsDirections.Add(dir);
					db.SaveChanges();
				}
			}

			Log("Locking doors");

			using (var db = new DataContext())
			{
				var areas = (from a in db.Areas select a).Include(a => a.Resets).ToArray();
				foreach (var area in areas)
				{
					foreach (var reset in area.Resets)
					{
						if (reset.ResetType != AreaResetType.Door)
						{
							continue;
						}

						if (reset.Value3 < 0 || reset.Value3 >= 6)
						{
							throw new Exception($"Reset {reset.Id}. Room direction with value {reset.Value3} is outside of range.");
						}

						var dir = (DirectionType)reset.Value3;

						var roomVnum = reset.Value2;

						var room = (from r in db.Rooms where r.VNum == roomVnum select r).Include(r => r.Exits).FirstOrDefault();
						if (room == null)
						{
							throw new Exception($"Reset {reset.Id}. Can't find room with vnum {roomVnum}");
						}

						var exit = (from e in room.Exits where e.DirectionType == (DirectionType)reset.Value3 select e).FirstOrDefault();
						if (exit == null || !exit.Flags.HasFlag(RoomDirectionFlags.Door))
						{
							throw new Exception($"Reset {reset.Id}. Can't find exit {dir}");
						}

						switch (reset.Value4)
						{
							case 0:
								break;
							case 1:
								exit.Flags |= RoomDirectionFlags.Closed;
								db.SaveChanges();
								break;
							case 2:
								exit.Flags |= RoomDirectionFlags.Closed | RoomDirectionFlags.Locked;
								db.SaveChanges();
								break;
							default:
								throw new Exception($"Reset {reset.Id}. Bad locks {reset.Value4}");

						}
					}
				}
			}

			Log("Success");
		}
	}
}