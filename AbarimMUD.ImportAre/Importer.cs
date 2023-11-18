using AbarimMUD.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			area.MaximumRooms = stream.ReadNumber();

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

				var name = stream.ReadDikuString();
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
					Material = stream.ReadEnumFromDikuString<Material>(),
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
						obj.Value4 = (int)stream.ReadSkill();
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Staff:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = stream.ReadNumber();
						obj.Value3 = stream.ReadNumber();
						obj.Value4 = (int)stream.ReadSkill();
						obj.Value5 = stream.ReadNumber();
						break;
					case ItemType.Potion:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadSkill();
						obj.Value3 = (int)stream.ReadSkill();
						obj.Value4 = (int)stream.ReadSkill();
						obj.Value5 = (int)stream.ReadSkill();
						break;
					case ItemType.Pill:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadSkill();
						obj.Value3 = (int)stream.ReadSkill();
						obj.Value4 = (int)stream.ReadSkill();
						obj.Value5 = (int)stream.ReadSkill();
						break;
					case ItemType.Scroll:
						obj.Value1 = stream.ReadNumber();
						obj.Value2 = (int)stream.ReadSkill();
						obj.Value3 = (int)stream.ReadSkill();
						obj.Value4 = (int)stream.ReadSkill();
						obj.Value5 = (int)stream.ReadSkill();
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


		/*		private void ProcessArea(DataContext db, Stream stream)
				{
					var line = reader.ReadLine();
					var vnum = line.ParseVnum();

					line = reader.ReadLine();
					var builder = line.RemoveTilda().Trim();

					line = reader.ReadLine();
					var name = line.RemoveTilda().Trim();

					line = reader.ReadLine();
					var parts = line.Split(" ");

					int startRoomVnum = int.Parse(parts[0]);
					int maximumRooms = int.Parse(parts[1]);
					int resetInMinutes = int.Parse(parts[2]);
					var resetMode = (ResetMode)int.Parse(parts[3]);
					int flags1 = 0;
					int flags2 = 0;
					int flags3 = 0;
					int flags4 = 0;
					int? minimumLevel = null;
					int? maximumLevel = null;
					if (parts.Length == 10)
					{
						// New TbaMUD
						flags1 = Utility.LoadFlags(parts[4]);
						flags2 = Utility.LoadFlags(parts[5]);
						flags3 = Utility.LoadFlags(parts[6]);
						flags4 = Utility.LoadFlags(parts[7]);

						minimumLevel = int.Parse(parts[8]);
						maximumLevel = int.Parse(parts[9]);
					}
					else if (parts.Length == 4)
					{
					}

					var created = false;
					var zone = (from z in db.Areas where z.VNum == vnum select z).FirstOrDefault();

					if (zone == null)
					{
						zone = new Area
						{
							VNum = vnum
						};
						db.Areas.Add(zone);
						created = true;
					}

					zone.Builder = builder;
					zone.Name = name;
					zone.StartRoomVNum = startRoomVnum;
					zone.MaximumRooms = maximumRooms;
					zone.ResetInMinutes = resetInMinutes;
					zone.ResetMode = resetMode;
					zone.Flags1 = flags1;
					zone.Flags2 = flags2;
					zone.Flags3 = flags3;
					zone.Flags4 = flags4;
					zone.MinimumLevel = minimumLevel;
					zone.MaximumLevel = maximumLevel;

					db.SaveChanges();

					if (created)
					{
						Log($"Created zone {vnum}, {builder}, {name}");
					}
					else
					{
						Log($"Updated zone {vnum}, {builder}, {name}");
					}
				}

				private void ProcessRoom(DataContext db, Stream stream)
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if (line.Trim() == "$~")
						{
							return;
						}

						var vnum = line.ParseVnum();

						// Find room zone
						var zone = (from z in db.Areas where z.StartRoomVNum <= vnum && vnum <= z.MaximumRooms select z).FirstOrDefault();
						if (zone == null)
						{
							throw new Exception($"Could not find zone for the room with vnum {vnum}");
						}

						var name = reader.ReadDikuString();
						var desc = reader.ReadDikuString();

						line = reader.ReadLine();
						var parts = line.Split(" ");

						var flags1 = int.Parse(parts[1]);
						var flags2 = int.Parse(parts[2]);
						var flags3 = int.Parse(parts[3]);
						var flags4 = int.Parse(parts[4]);
						var sectorType = int.Parse(parts[5]);

						if (sectorType > 10)
						{
							sectorType = 0;
						}

						var created = false;
						var room = (from r in db.Rooms where r.VNum == vnum select r)
							.FirstOrDefault();

						if (room == null)
						{
							room = new Room
							{
								VNum = vnum,
							};

							db.Rooms.Add(room);
							created = true;
						}

						room.Area = zone;
						room.AreaId = zone.Id;
						room.Name = name;
						room.Description = desc;
						room.Flags1 = flags1;
						room.Flags2 = flags2;
						room.Flags3 = flags3;
						room.Flags4 = flags4;
						room.SectorType = sectorType;

						if (created)
						{
							Log($"Created room {vnum}, {name}");
						}
						else
						{
							Log($"Updated room {vnum}, {name}");
						}

						// Delete room directions
						if (!created)
						{
							db.Database.ExecuteSqlRaw("DELETE FROM RoomsDirections WHERE SourceRoomId={0}", room.Id);
							db.Database.ExecuteSqlRaw("DELETE FROM RoomsDirections WHERE TargetRoomId={0}", room.Id);
						}

						db.SaveChanges();

						while (!reader.EndOfStream)
						{
							line = reader.ReadLine();

							switch (line[0])
							{
								case 'D':

									var roomDirection = new RoomDirection
									{
										SourceRoomId = room.Id,
										DirectionType = (DirectionType)int.Parse(line[1].ToString()),
										GeneralDescription = reader.ReadDikuString(),
										Keyword = reader.ReadDikuString()
									};

									line = reader.ReadLine();
									parts = line.Split(' ');

									var flags = RoomDirectionFlags.None;
									var val = int.Parse(parts[0]);
									switch (val)
									{
										case 1:
											flags = RoomDirectionFlags.IsDoor;
											break;
										case 2:
											flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.PickProof;
											break;
										case 3:
											flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.Hidden;
											break;
										case 4:
											flags = RoomDirectionFlags.IsDoor | RoomDirectionFlags.PickProof | RoomDirectionFlags.Hidden;
											break;
										default:
											break;
									}

									roomDirection.Flags = flags;

									val = int.Parse(parts[1]);
									if (val != -1 && val != 65535)
									{
										roomDirection.Key = val;
									}

									val = int.Parse(parts[2]);
									if (val != -1 && val != 0)
									{
										roomDirection.ToRoom = val;
									}

									_tempDirections.Add(roomDirection);
									break;

								case 'E':
									room.NDKeyword = reader.ReadDikuString();
									room.NDDescription = reader.ReadDikuString();
									break;

								case 'S':
									// Process triggers
									while (!reader.EndOfStream)
									{
										var c = (char)reader.Peek();
										if (c != 'T')
										{
											break;
										}

										line = reader.ReadLine();
									}

									goto finishRoom;
							}
						}
					finishRoom:
						db.SaveChanges();
					}
				}*/

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

			var tasks = new List<Action>();
			foreach (var areaFile in areaFiles)
			{
				var task = () => ProcessFile(areaFile);
				tasks.Add(task);
			}

			// Parallel.Invoke(tasks.ToArray());
			foreach (var task in tasks)
			{
				task();
			}

			/*			ProcessType("Areas", "zon", ProcessArea);
						ProcessType("Rooms", "wld", ProcessRoom);

						// Process directions
						Log("Updating directions");
						var tasks = new List<Action>();
						foreach (var dir in _tempDirections)
						{
							var task = () =>
							{
								using (var db = new DataContext())
								{
									if (dir.ToRoom != null)
									{
										dir.TargetRoomId = (from r in db.Rooms where r.VNum == dir.ToRoom.Value select r).First().Id;
									} else
									{
										dir.TargetRoomId = null;
									}
									db.RoomsDirections.Add(dir);
									db.SaveChanges();
								}
							};

							tasks.Add(task);
						}

						Parallel.Invoke(tasks.ToArray());*/

			Log("Success");
		}
	}
}
