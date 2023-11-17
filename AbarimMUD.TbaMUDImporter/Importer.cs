using AbarimMUD.Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.TbaMUDImporter
{
	internal class Importer
	{
		private const string InputDir = "D:\\Projects\\chaos\\tbamud\\lib";

		private DataContext _db;
		private string[] _indexFiles;
		private readonly List<RoomDirection> _tempDirections = new List<RoomDirection>();

		private static void Log(string message)
		{
			Console.WriteLine(message);
		}


		private void LoadIndexFiles()
		{
			_indexFiles = Directory.EnumerateFiles(InputDir, "index*", SearchOption.AllDirectories).ToArray();
		}

		private string[] EnumerateFilesFromIndex(string folderName, out string indexFile)
		{
			indexFile = (from f in _indexFiles where f.Replace("\\", "/").EndsWith(folderName + "/index") select f).FirstOrDefault();
			if (string.IsNullOrEmpty(indexFile))
			{
				throw new Exception($"Could not find zone index file('{folderName}/index')");
			}

			var result = new List<string>();
			using (var stream = File.OpenRead(indexFile))
			using (var reader = new StreamReader(stream))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (line.Trim() == "$")
					{
						break;
					}

					result.Add(line);
				}
			}

			return result.ToArray();
		}

		private void ProcessType(string typeName, string folderName, Action<StreamReader> processor)
		{
			Log($"Processing {typeName}...");

			string indexFile;
			var files = EnumerateFilesFromIndex(folderName, out indexFile);
			var folder = Path.GetDirectoryName(indexFile);
			foreach (var file in files)
			{
				using (_db = new DataContext())
				{
					var filePath = Path.Combine(folder, file);
					Log($"Processing {filePath}...");

					using (var stream = File.OpenRead(filePath))
					using (var reader = new StreamReader(stream))
					{
						processor(reader);
					}
				}
			}
		}

		private void ProcessZone(StreamReader reader)
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
			long flags1 = 0;
			long flags2 = 0;
			long flags3 = 0;
			long flags4 = 0;
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
			var zone = (from z in _db.Zones where z.VNum == vnum select z).FirstOrDefault();

			if (zone == null)
			{
				zone = new Zone
				{
					VNum = vnum
				};
				_db.Zones.Add(zone);
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

			_db.SaveChanges();

			if (created)
			{
				Log($"Created zone {vnum}, {builder}, {name}");
			}
			else
			{
				Log($"Updated zone {vnum}, {builder}, {name}");
			}
		}

		private void ProcessRoom(StreamReader reader)
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
				var zone = (from z in _db.Zones where z.StartRoomVNum <= vnum && vnum <= z.MaximumRooms select z).FirstOrDefault();
				if (zone == null)
				{
					throw new Exception($"Could not find zone for the room with vnum {vnum}");
				}

				var name = reader.ReadDikuString();
				var desc = reader.ReadDikuString();

				line = reader.ReadLine();
				var parts = line.Split(" ");

				var flags1 = long.Parse(parts[1]);
				var flags2 = long.Parse(parts[2]);
				var flags3 = long.Parse(parts[3]);
				var flags4 = long.Parse(parts[4]);
				var sectorType = int.Parse(parts[5]);

				if (sectorType > 10)
				{
					sectorType = 0;
				}

				var created = false;
				var room = (from r in _db.Rooms where r.VNum == vnum select r).FirstOrDefault();

				if (room == null)
				{
					room = new Room
					{
						VNum = vnum,
					};

					_db.Rooms.Add(room);
					created = true;
				}

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
				if (room.InputDirections.Count > 0)
				{
					_db.RoomsDirections.RemoveRange(room.InputDirections);
				}

				if (room.OutputDirections.Count > 0)
				{
					_db.RoomsDirections.RemoveRange(room.OutputDirections);
				}

				_db.SaveChanges();

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

							if (roomDirection.ToRoom != null)
							{
								_tempDirections.Add(roomDirection);
							}
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
				_db.SaveChanges();
			}
		}

		public void Process()
		{
			LoadIndexFiles();

			ProcessType("Zones", "zon", ProcessZone);
			ProcessType("Rooms", "wld", ProcessRoom);

			// Process directions
			Log("Updating directions");
			using (_db = new DataContext())
			{
				foreach (var dir in _tempDirections)
				{
					dir.TargetRoomId = (from r in _db.Rooms where r.VNum == dir.ToRoom select r).First().Id;
					_db.RoomsDirections.Add(dir);
				}

				_db.SaveChanges();
			}

			Log("Success");
		}
	}
}
