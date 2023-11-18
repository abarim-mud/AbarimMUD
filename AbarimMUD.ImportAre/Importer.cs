using AbarimMUD.Common.Data;
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
			area.Builder = stream.ReadDikuString();

			area.StartRoomVNum = stream.ReadNumber();
			area.EndRoomVNum = stream.ReadNumber();

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
					Flags = stream.ReadFlag(),
					AffectedBy = stream.ReadFlag(),
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
					OffenseFlags = stream.ReadFlag(),
					ImmuneFlags = stream.ReadFlag(),
					ResistanceFlags = stream.ReadFlag(),
					VulnerableFlags = stream.ReadFlag(),
					StartPosition = stream.ReadEnumFromWord<MobilePosition>(),
					DefaultPosition = stream.ReadEnumFromWord<MobilePosition>(),
					Sex = stream.ReadEnumFromWord<Sex>(),
					Wealth = stream.ReadNumber(),
					FormsFlags = stream.ReadFlag(),
					PartsFlags = stream.ReadFlag(),
					Size = stream.ReadEnumFromWord<MobileSize>(),
					Material = stream.ReadEnumFromWord<Material>(),
				};

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'F')
					{
						var word = stream.ReadWord();
						var vector = stream.ReadFlag();
					}
					else if (c == 'M')
					{
						var word = stream.ReadWord();
						var mnum = stream.ReadNumber();
						var trig = stream.ReadDikuString();
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
					Material = stream.ReadEnumFromDikuStringWithDef<Material>(Material.Generic),
					ItemType = stream.ReadEnumFromWord<ItemType>(),
					ExtraFlags = stream.ReadFlag(),
					WearFlags = stream.ReadFlag(),
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

				while (!stream.EndOfStream())
				{
					var c = stream.ReadSpacedLetter();

					if (c == 'A')
					{
						var location = stream.ReadNumber();
						var modifier = stream.ReadNumber();
					}
					else if (c == 'F')
					{
						c = stream.ReadSpacedLetter();
						var location = stream.ReadNumber();
						var modifier = stream.ReadNumber();

						var bits = stream.ReadFlag();
					}
					else if (c == 'E')
					{
						var keyword = stream.ReadDikuString();
						var description = stream.ReadDikuString();
					}
					else
					{
						stream.GoBackIfNotEOF();
						break;
					}
				}

				db.Objects.Add(obj);
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
							AreaId = area.Id,
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

						roomDirection.Key = stream.ReadNumber();
						roomDirection.TargetRoomVNum = stream.ReadNumber();

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
					if (dir.TargetRoomVNum != -1)
					{
						dir.TargetRoomId = (from r in db.Rooms where r.VNum == dir.TargetRoomVNum select r).First().Id;
					}
					else
					{
						dir.TargetRoomId = null;
					}
					db.RoomsDirections.Add(dir);
					db.SaveChanges();
				}
			}

			Log("Success");
		}
	}
}