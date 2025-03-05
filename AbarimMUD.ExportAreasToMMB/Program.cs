using System.Collections.Generic;
using System.IO;
using System;
using MUDMapBuilder;
using AbarimMUD.Storage;
using System.Linq;
using AbarimMUD.Data;
using System.Drawing;

namespace AbarimMUD.ExportAreasToMMB
{
	internal static class Program
	{
		static void Log(string message) => Console.WriteLine(message);

		static void Process(string inputFolder, string outputFolder)
		{
			StorageUtility.InitializeStorage(Log);

			DataContext.Load(inputFolder);

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
					var newMobile = new MobileInstance(mobile)
					{
						Room = room
					};
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

			// Save all areas and generate conversion script
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}

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
