using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum SectorType
	{
		Inside,
		City,
		Field,
		Forest,
		Hills,
		Mountain,
		WaterNoSwim,
		Air,
		Desert,
		River,
		Cave,
		Swim,
		Swamp,
		Underground,
		Trail,
		Road,
		Ocean
	}

	public enum RoomFlags
	{
		NoMob,
		Dark,
		Indoors,
		Law,
		Private,
		NoRecall,
		Solitary,
		HeroesOnly,
		GodsOnly,
		ImpOnly,
		Safe,
		PetShop,
		NewbiesOnly,
		Nowhere,
	}

	public class Room : AreaEntity
	{
		private static bool _coordsDirty = true;

		public string Name { get; set; }
		public string Description { get; set; }

		[Browsable(false)]
		public HashSet<RoomFlags> Flags { get; set; }
		public SectorType SectorType { get; set; }
		public int HealRate { get; set; }
		public int ManaRate { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
		public string Owner { get; set; }

		[Browsable(false)]
		public Dictionary<Direction, RoomExit> Exits { get; set; } = new Dictionary<Direction, RoomExit>();

		[Browsable(false)]
		[JsonIgnore]
		public List<MobileInstance> Mobiles { get; } = new List<MobileInstance>();

		[Browsable(false)]
		[JsonIgnore]
		public List<Character> Characters { get; } = new List<Character>();

		[Browsable(false)]
		[JsonIgnore]
		public int X, Y, Z;


		public void AddCharacter(Character character)
		{
			// Remove all characters with such id
			Characters.RemoveAll(character1 => character1.Name == character.Name);
			Characters.Add(character);
		}

		public void RemoveCharacter(Character character)
		{
			Characters.Remove(character);
		}

		public override string ToString() => $"{Name} (#{Id})";

		public void DisconnectRoom(Direction direction, bool updateArea = true)
		{
			var oppositeDir = direction.GetOppositeDirection();

			// Get the connection
			RoomExit existingConnection;
			if (!Exits.TryGetValue(direction, out existingConnection))
			{
				return;
			}

			if (existingConnection != null)
			{
				Exits.Remove(direction);
				if (existingConnection.TargetRoom != null)
				{
					RoomExit oppositeConnection;
					if (existingConnection.TargetRoom.Exits.TryGetValue(oppositeDir, out oppositeConnection) &&
						oppositeConnection.TargetRoom == this)
					{
						existingConnection.TargetRoom.Exits.Remove(oppositeDir);
					}
				}

				if (updateArea)
				{
					Area.Save();
				}
			}

			InvalidateCoords();
		}

		public void ConnectRoom(Room targetRoom, Direction direction)
		{
			// Delete existing connections
			DisconnectRoom(direction, false);

			// Create new ones
			var newConnection = new RoomExit
			{
				TargetRoom = targetRoom,
				Direction = direction
			};

			Exits[direction] = newConnection;

			var oppositeNewConnection = new RoomExit
			{
				TargetRoom = this,
				Direction = direction.GetOppositeDirection()
			};
			targetRoom.Exits[oppositeNewConnection.Direction] = oppositeNewConnection;

			Area.Save();
			if (Area != targetRoom.Area)
			{
				targetRoom.Area.Save();
			}

			InvalidateCoords();
		}

		public void Delete()
		{
			// Firstly delete all connections to this room
			foreach (var area in Area.Storage)
			{
				foreach (var room in area.Rooms)
				{
					if (room.Id == Id)
					{
						continue;
					}

					var toDelete = new List<Direction>();
					foreach (var pair in room.Exits)
					{
						if (pair.Value.TargetRoom != null && pair.Value.TargetRoom.Id == Id)
						{
							toDelete.Add(pair.Key);
						}
					}

					foreach (var td in toDelete)
					{
						room.Exits.Remove(td);
					}
				}
			}

			// Now remove room from the area
			if (Area != null)
			{
				Area.Rooms.Remove(this);
			}

			// Finally save the area
			Area.Save();
		}

		public static Room GetRoomById(int id) => Area.Storage.GetRoomById(id);
		public static Room EnsureRoomById(int id) => Area.Storage.EnsureRoomById(id);

		private static void InvalidateCoords()
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

			while(data.Count > 0)
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
		/// Find first direction to shortest path between two rooms using Dijkstra algorithm
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

			var data = new PriorityQueue<Tuple<Direction, Room>, int>();

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
				data.Enqueue(new Tuple<Direction, Room>(pair.Key, targetRoom), dist);
			}

			while (data.Count > 0)
			{
				var roomData = data.Dequeue();
				var dir = roomData.Item1;
				var room = roomData.Item2;

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
					data.Enqueue(new Tuple<Direction, Room>(dir, targetRoom), dist);
				}
			}

			Debug.WriteLine($"FindFirstStep: Target room isn't reachable. Spent {step} steps.");

			return null;
		}
	}

	public static class RoomUtils
	{
		private struct SectorData
		{
			public int MovementCost;
			public int MovementWait;

			public SectorData(int movementCost, int movementWait)
			{
				MovementCost = movementCost;
				MovementWait = movementWait;
			}
		}

		private static readonly Dictionary<SectorType, SectorData> _sectorData = new Dictionary<SectorType, SectorData>();

		static RoomUtils()
		{
			_sectorData[SectorType.Inside] = new SectorData(1, 1);
			_sectorData[SectorType.City] = new SectorData(1, 1);
			_sectorData[SectorType.Field] = new SectorData(2, 1);
			_sectorData[SectorType.Forest] = new SectorData(5, 2);
			_sectorData[SectorType.Hills] = new SectorData(3, 4);
			_sectorData[SectorType.Mountain] = new SectorData(6, 3);
			_sectorData[SectorType.WaterNoSwim] = new SectorData(6, 3);
			_sectorData[SectorType.Air] = new SectorData(10, 1);
			_sectorData[SectorType.Desert] = new SectorData(2, 2);
			_sectorData[SectorType.River] = new SectorData(6, 2);
			_sectorData[SectorType.Cave] = new SectorData(3, 3);
			_sectorData[SectorType.Swim] = new SectorData(6, 2);
			_sectorData[SectorType.Swamp] = new SectorData(3, 3);
			_sectorData[SectorType.Underground] = new SectorData(2, 2);
			_sectorData[SectorType.Trail] = new SectorData(2, 1);
			_sectorData[SectorType.Road] = new SectorData(1, 1);
			_sectorData[SectorType.Ocean] = new SectorData(10, 3);
		}

		private static SectorData EnsureSectorData(this SectorType sectorType)
		{
			SectorData sd;
			if (!_sectorData.TryGetValue(sectorType, out sd))
			{
				throw new Exception($"Unknown sector type {sectorType}");
			}

			return sd;
		}

		public static int GetMovementCost(this SectorType sectorType) => sectorType.EnsureSectorData().MovementCost;
		public static int GetMovementCost(this Room room) => room.SectorType.GetMovementCost();

		public static int GetMovementWait(this SectorType sectorType) => sectorType.EnsureSectorData().MovementWait;
		public static int GetMovementWait(this Room room) => room.SectorType.GetMovementWait();
	}
}
