using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using MUDMapBuilder;
using AbarimMUD.Storage;
using System.Linq;
using AbarimMUD.Data;

namespace AbarimMUD.ExportAreasToMMB
{
	internal static class Program
	{
		static void Log(string message) => Console.WriteLine(message);

		static void Process(string inputFolder, string outputFolder)
		{
			DataContext.Initialize(inputFolder, Log);

			DataContext.Register(Race.Storage);
			DataContext.Register(MobileClass.Storage);
			DataContext.Register(Mobile.Storage);
			DataContext.Register(Item.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();

			var mmbAreas = new List<MMBArea>();
			foreach (var area in Area.Storage)
			{
				Log($"Processing area '{area.Name}'");

				var mmbArea = new MMBArea
				{
					Name = area.Name
				};

				foreach(var room in area.Rooms)
				{
					var mmbRoom = new MMBRoom(room.Id, room.Name, false);
					Log($"{mmbRoom}");
					mmbArea.Add(mmbRoom);

					foreach (var exit in room.Exits)
					{
						if (exit.Value == null || exit.Value.TargetRoom == null)
						{
							continue;
						}

						var dir = (MMBDirection)exit.Key;
						var targetRoomId = exit.Value.TargetRoom.Id;
						mmbRoom.Connections[dir] = new MMBRoomConnection(dir, targetRoomId);
					}
				}

				mmbAreas.Add(mmbArea);
			}

			// Build complete dictionary of rooms
			var allRooms = new Dictionary<int, MMBRoom>();
			foreach (var area in mmbAreas)
			{
				foreach (var room in area.Rooms)
				{
					if (allRooms.ContainsKey(room.Id))
					{
						throw new Exception($"Dublicate room id: {room.Id}");
					}

					var areaExit = room.Clone();
					areaExit.Name = $"To {area.Name}";
					areaExit.IsExitToOtherArea = true;

					allRooms[room.Id] = areaExit;
				}
			}

			// Now add areas exits
			foreach (var area in mmbAreas)
			{
				var areaExits = new Dictionary<int, MMBRoom>();
				foreach (var room in area.Rooms)
				{
					var toDelete = new List<MMBDirection>();
					foreach (var exit in room.Connections)
					{
						if (!allRooms.ContainsKey(exit.Value.RoomId))
						{
							toDelete.Add(exit.Key);
						}
					}

					foreach (var d in toDelete)
					{
						room.Connections.Remove(d);
					}

					foreach (var exit in room.Connections)
					{
						MMBRoom inAreaRoom;
						inAreaRoom = (from r in area.Rooms where r.Id == exit.Value.RoomId select r).FirstOrDefault();
						if (inAreaRoom != null)
						{
							continue;
						}

						areaExits[exit.Value.RoomId] = allRooms[exit.Value.RoomId];
					}
				}

				foreach (var pair in areaExits)
				{
					area.Add(pair.Value);
				}
			}

			// Save all areas and generate conversion script
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}

			foreach (var area in mmbAreas)
			{
				var fileName = $"{area.Name}.json";
				Log($"Saving {fileName}...");

				// Copy build options
				var project = new MMBProject(area, new BuildOptions());
				var data = project.ToJson();
				File.WriteAllText(Path.Combine(outputFolder, fileName), data);
			}
		}

		static void Main(string[] args)
		{
			try
			{
				Process(@"D:\Projects\AbarimMUD\Data", @"D:\Projects\abarim-mud.github.io\maps\json");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}

}
