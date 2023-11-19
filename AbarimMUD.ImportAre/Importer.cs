﻿using AbarimMUD.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AbarimMUD.ImportAre
{
	internal class Importer
	{
		private const string InputDir = "D:\\Projects\\chaos\\basedmud\\area";

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

			area.StartRoomVNum = stream.ReadNumber();
			area.EndRoomVNum = stream.ReadNumber();

			db.Areas.Add(area);
			db.SaveChanges();

			Log($"Created area {area.Name}");
		}

		private void ProcessAreaData(DataContext db, Stream stream, Area area)
		{
			while(true)
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
						area.Security = stream.ReadNumber();
						break;
					case 'V':
						if (word == "VNUMs")
						{
							area.StartRoomVNum = stream.ReadNumber();
							area.EndRoomVNum = stream.ReadNumber();
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

						switch(word.Substring(0, 3).ToLower())
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
							EffectType = (EffectType)stream.ReadNumber(),
							Modifier = stream.ReadNumber()
						};

						db.ObjectsEffect.Add(effect);
					}
					else if (c == 'F')
					{
						c = stream.ReadSpacedLetter();
						var location = (EffectType)stream.ReadNumber();
						var modifier = stream.ReadNumber();

						var bits = stream.ReadFlag();
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

				room.Flags = stream.ReadFlag();
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
								roomDirection.Flags = RoomDirectionFlags.IsDoor;
								break;
							case 2:
								roomDirection.Flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.PickProof;
								break;
							case 3:
								roomDirection.Flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.NoPass;
								break;
							case 4:
								roomDirection.Flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.PickProof | RoomDirectionFlags.NoPass;
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

		private void ProcessFile(string areaFile)
		{
			Log($"Processing {areaFile}...");
			using (var db = new DataContext())
			{
				Area zone = null;
				using (var stream = File.OpenRead(areaFile))
				{
					while (!stream.EndOfStream())
					{
						var type = stream.ReadId();
						switch (type)
						{
							case "AREA":
								zone = new Area();
								ProcessArea(db, stream, zone);
								break;
							case "AREADATA":
								zone = new Area();
								ProcessAreaData(db, stream, zone);
								break;
							case "MOBILES":
								ProcessMobiles(db, stream, zone);
								break;
							case "OBJECTS":
								ProcessObjects(db, stream, zone);
								break;
							case "ROOMS":
								ProcessRooms(db, stream, zone);
								break;
							default:
								goto finish;
								break;
						}
					}

				finish:;
				}
			}
		}

		public void Process()
		{
			var areaFiles = Directory.EnumerateFiles(InputDir, "*.are", SearchOption.AllDirectories).ToArray();

			// Recreate the db
			using (var db = new DataContext())
			{
				db.Database.EnsureDeleted();
				db.Database.EnsureCreated();
			}

			foreach (var areaFile in areaFiles)
			{
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

			Log("Success");
		}
	}
}