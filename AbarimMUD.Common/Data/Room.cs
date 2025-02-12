using System.Collections.Generic;
using System.ComponentModel;
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
		Unused,
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
	}
}
