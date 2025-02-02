using AbarimMUD.Data;
using MUDMapBuilder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AbarimMUD.MapViewer
{
	internal static class Utility
	{
		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static string Version
		{
			get
			{
				var assembly = typeof(Utility).Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

		public static void QueueUIAction(Action action) => EditorGame.Instance.QueueUIAction(action);

		public static void SetStatusMessage(string message) => EditorGame.Instance.SetStatusMessage(message);

		public static List<MMBArea> BuildMMBAreas()
		{
			// Build dict of all mobiles
			var areas = new List<MMBArea>();
			var allMobiles = new Dictionary<int, Mobile>();
			foreach (var sourceArea in Area.Storage)
			{
				if (sourceArea.Rooms == null || sourceArea.Rooms.Count == 0)
				{
					continue;
				}

				var mmbArea = new MMBArea
				{
					Name = sourceArea.Name,
					Tag = sourceArea
				};

				foreach (var room in sourceArea.Rooms)
				{
					var mmbRoom = new MMBRoom(room.Id, room.Name)
					{
						Tag = room
					};

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

				areas.Add(mmbArea);

				foreach (var mobile in sourceArea.Mobiles)
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

					areaExit.Name = $"To {area.Name}";
					areaExit.Color = Color.Blue;
					areaExit.FrameColor = Color.Blue;

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
			foreach (var sourceArea in Area.Storage)
			{
				foreach (var reset in sourceArea.MobileResets)
				{
					Mobile mobile;
					if (!allMobiles.TryGetValue(reset.MobileId, out mobile))
					{
						Console.WriteLine($"Warning: Unable to find mobile with vnum {reset.MobileId}.");
						continue;
					}

					MMBRoom room;
					if (!allRooms.TryGetValue(reset.RoomId, out room))
					{
						Console.WriteLine($"Warning: Unable to find room with vnum {reset.RoomId}.");
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

					var mobileName = $"{mobile.ShortDescription} #{mobile.Id}";

					if (mobile.Guildmaster != null)
					{
						mobileName += $"({mobile.Guildmaster.Name} gm)";
					}

					room.Contents.Add(new MMBRoomContentRecord(mobileName, color));
				}
			}

			return areas;
		}
	}
}
