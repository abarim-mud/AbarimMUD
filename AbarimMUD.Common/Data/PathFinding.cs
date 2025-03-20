using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AbarimMUD.Data
{
	public static class PathFinding
	{
		public class PathFindingResult
		{
			public Direction FirstDirection { get; }
			public int StepsCount { get; }

			internal PathFindingResult(Direction firstDirection, int stepsCount)
			{
				FirstDirection = firstDirection;
				StepsCount = stepsCount;
			}
		}


		private class Node
		{
			public Room Room { get; }
			public Direction SourceDirection { get; }
			public Node Source { get; }

			public Node(Room room, Direction sourceDirection, Node source)
			{
				Room = room ?? throw new ArgumentNullException(nameof(room));
				SourceDirection = sourceDirection;
				Source = source;
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
		/// Find path(not necessarily shortest) between two rooms using simple algorithm with distance heuristics
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static PathFindingResult BuildPath(Room source, Room target)
		{
			if (source.Id == target.Id)
			{
				return null;
			}

			PathFindingResult result = null;

			UpdateCoords();

			var s = Stopwatch.StartNew();

			// Enqueue first room, the direction isn't important
			var visited = new HashSet<int>
			{
				source.Id
			};

			var data = new PriorityQueue<Node, int>();
			data.Enqueue(new Node(source, Direction.North, null), Distance(source, target));

			var step = 0;
			while (data.Count > 0)
			{
				var node = data.Dequeue();
				var room = node.Room;

				if (room.Id == target.Id)
				{
					// Reached
					Debug.WriteLine($"BuildPathAsync: Found required room at {step} step.");

					// Go back to the start, counting steps
					var moveSteps = 1;
					var n = node;
					while (n.Source != null && n.Source.Room.Id != source.Id)
					{
						++moveSteps;
						n = n.Source;
					}

					if (n.Source == null)
					{
						// Should never happen
						Debug.Assert(false);
						break;
					}

					result = new PathFindingResult(n.SourceDirection, moveSteps);
					break;
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
					data.Enqueue(new Node(targetRoom, pair.Key, node), dist);
				}
			}

			s.Stop();

			if (result == null)
			{
				Debug.WriteLine($"BuildPathAsync: Destination room unreachable at {step} step.");
			}

			Debug.WriteLine($"Took {s.ElapsedMilliseconds} ms");

			return result;
		}
	}
}
