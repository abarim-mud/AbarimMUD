using System;

namespace AbarimMUD.Data
{
	[Flags]
	public enum RoomExitFlags
	{
		None = 0,
		Door = 1 << 0,
		Closed = 1 << 1,
		Locked = 1 << 2,
		PickProof = 1 << 5,
		NoPass = 1 << 6,
		Easy = 1 << 7,
		Hard = 1 << 8,
		Infuriating = 1 << 9,
		NoClose = 1 << 10,
		NoLock = 1 << 11,
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

	public class RoomExit : Entity
	{
		public int SourceRoomId { get; set; }
		public Room SourceRoom { get; set; }
		public int? TargetRoomId { get; set; }
		public Room TargetRoom { get; set; }
		public Direction Direction { get; set; }
		public string Description { get; set; }
		public string Keyword { get; set; }
		public RoomExitFlags Flags { get; set; }
		public int? KeyObjectId { get; set; }
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
