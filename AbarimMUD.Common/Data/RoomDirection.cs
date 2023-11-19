using System;

namespace AbarimMUD.Common.Data
{
	[Flags]
	public enum RoomDirectionFlags
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

	public enum DirectionType
	{
		North,
		East,
		South,
		West,
		Up,
		Down,
	}

	public class RoomDirection: Entity
	{
		public int SourceRoomId { get; set; }
		public Room SourceRoom { get; set; }
		public int? TargetRoomId { get; set; }
		public Room TargetRoom { get; set; }
		public DirectionType DirectionType { get; set; }
		public string Description { get; set; }
		public string Keyword { get; set; }
		public RoomDirectionFlags Flags { get; set; }
		public int? KeyObjectId { get; set; }
		public GameObject KeyObject { get; set; }
		public int? KeyObjectVNum { get; set; }
		public int? TargetRoomVNum { get; set; }
	}
}
