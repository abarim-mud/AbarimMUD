using System.Collections.Generic;
using System.Diagnostics;

namespace AbarimMUD.Data
{
	public static class PathFinding
	{
		private struct RoomDir
		{
			public Direction InitialDirection;
			public Room Room;

			public RoomDir(Direction initialDirection, Room room)
			{
				InitialDirection = initialDirection;
				Room = room;
			}
		}

		private static bool _coordsDirty = true;

		internal static void InvalidateCoords()
		{
			_coordsDirty = true;
		}

		private static void UpdateCoords()
		{
			if (!_coordsDirty)
			{
				return;
			}

			Debug.WriteLine("Rebuilding rooms coordinates");

			var count = 1;
			var startingRoom = Room.EnsureRoomById(Configuration.StartRoomId);
			startingRoom.X = startingRoom.Y = startingRoom.Z = 0;

			var data = new Queue<Room>();
			data.Enqueue(startingRoom);

			var visited = new HashSet<int>
			{
				startingRoom.Id
			};

			while (data.Count > 0)
			{
				var room = data.Dequeue();

				foreach (var pair in room.Exits)
				{
					if (pair.Value == null || pair.Value.TargetRoom == null || visited.Contains(pair.Value.TargetRoom.Id))
					{
						continue;
					}

					var targetRoom = pair.Value.TargetRoom;
					visited.Add(targetRoom.Id);

					targetRoom.X = room.X;
					targetRoom.Y = room.Y;
					targetRoom.Z = room.Z;

					switch (pair.Key)
					{
						case Direction.North:
							--targetRoom.Z;
							break;
						case Direction.East:
							++targetRoom.X;
							break;
						case Direction.South:
							++targetRoom.Z;
							break;
						case Direction.West:
							--targetRoom.X;
							break;
						case Direction.Up:
							++targetRoom.Y;
							break;
						case Direction.Down:
							--targetRoom.Y;
							break;
					}

					++count;

					data.Enqueue(targetRoom);
				}
			}

			_coordsDirty = false;

			Debug.WriteLine($"Coords are set for {count} rooms.");
		}

		private static int Distance(Room room1, Room room2)
		{
			var dx = room1.X - room2.X;
			var dy = room1.Y - room2.Y;
			var dz = room1.Z - room2.Z;

			return dx * dx + dy * dy + dz * dz;
		}

		/// <summary>
		/// Find first direction to path(not necessarily shortest) between two rooms using simple algorithm with distance heuristics
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Direction? FindFirstStep(Room source, Room target)
		{
			if (source.Id == target.Id)
			{
				return null;
			}

			UpdateCoords();

			var visited = new HashSet<int>
			{
				source.Id
			};

			var data = new PriorityQueue<RoomDir, int>();

			var step = 0;

			// Enqueue first rooms and dirs
			foreach (var pair in source.Exits)
			{
				if (pair.Value == null || pair.Value.TargetRoom == null || visited.Contains(pair.Value.TargetRoom.Id))
				{
					continue;
				}

				var targetRoom = pair.Value.TargetRoom;
				visited.Add(targetRoom.Id);

				var dist = Distance(targetRoom, target);
				data.Enqueue(new RoomDir(pair.Key, targetRoom), dist);
			}

			while (data.Count > 0)
			{
				var roomData = data.Dequeue();
				var dir = roomData.InitialDirection;
				var room = roomData.Room;

				if (room.Id == target.Id)
				{
					// Reached
					Debug.WriteLine($"FindFirstStep: Found required room at {step} step.");

					// This  should store direction from the source room
					return dir;
				}

				++step;

				foreach (var pair in room.Exits)
				{
					if (pair.Value == null || pair.Value.TargetRoom == null || visited.Contains(pair.Value.TargetRoom.Id))
					{
						continue;
					}

					var targetRoom = pair.Value.TargetRoom;
					visited.Add(targetRoom.Id);

					var dist = Distance(targetRoom, target);
					data.Enqueue(new RoomDir(dir, targetRoom), dist);
				}
			}

			Debug.WriteLine($"FindFirstStep: Target room isn't reachable. Spent {step} steps.");

			return null;
		}
	}
}
