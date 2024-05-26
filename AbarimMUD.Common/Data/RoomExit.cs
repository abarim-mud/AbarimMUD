using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum RoomExitFlags
	{
		Door,
		Closed,
		Locked,
		PickProof,
		NoPass,
		Window,
		NonObvious,
		NoBash,
	}

	public enum Direction
	{
		North,
		East,
		South,
		West,
		Up,
		Down,
	}

	public class RoomExit
	{
		[JsonIgnore]
		public Room TargetRoom { get; set; }
		
		[JsonIgnore]
		public Direction Direction { get; set; }

		public string Display { get; set; }
		public string Description { get; set; }

		public string Keyword { get; set; }

		public HashSet<RoomExitFlags> Flags { get; set; }

		public int? KeyObjectId { get; set; }

		public object Tag { get; set; }
	}

	public static class RoomDirectionExtensions
	{
		public static Direction GetOppositeDirection(this Direction direction)
		{
			switch (direction)
			{
				case Direction.East:
					return Direction.West;
				case Direction.West:
					return Direction.East;
				case Direction.North:
					return Direction.South;
				case Direction.South:
					return Direction.North;
				case Direction.Up:
					return Direction.Down;
				default:
					return Direction.Up;
			}
		}

		public static string GetName(this Direction direction) => direction.ToString().ToLower();
	}
}
