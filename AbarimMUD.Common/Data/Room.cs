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

			// We go from target to source
			// So the last step would return the desired direction
			var data = new SortedDictionary<int, Queue<Room>>
			{
				[0] = new Queue<Room>()
			};

			data[0].Enqueue(target);

			var visited = new HashSet<int>();

			var step = 0;
			while (data.Count > 0)
			{
				var top = data.First();

				var room = top.Value.Dequeue();
				if (top.Value.Count == 0)
				{
					// Remove empty list
					data.Remove(top.Key);
				}

				++step;

				var newDist = top.Key + 1;

				foreach (var pair in room.Exits)
				{
					if (pair.Value == null || pair.Value.TargetRoom == null || visited.Contains(pair.Value.TargetRoom.Id))
					{
						continue;
					}

					var targetRoom = pair.Value.TargetRoom;
					if (targetRoom.Id == source.Id)
					{
						// Reached
						Debug.WriteLine($"FindFirstStep: Found required room at {step} step.");

						// Return opposite direction
						return pair.Key.GetOppositeDirection();
					}

					visited.Add(targetRoom.Id);

					Queue<Room> rooms;
					if (!data.TryGetValue(newDist, out rooms))
					{
						rooms = new Queue<Room>();
						data[newDist] = rooms;
					}

					rooms.Enqueue(targetRoom);
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
