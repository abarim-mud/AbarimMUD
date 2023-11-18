using System;

namespace AbarimMUD.Common.Data
{
	[Flags]
	public enum RoomDirectionFlags
	{
		None = 0,
		IsDoor = 1 << 0,
		PickProof = 1 << 1,
		NoPass = 1 << 2,
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
		public int AreaId { get; set; }
		public Area Area { get; set; }
		public int SourceRoomId { get; set; }
		public Room SourceRoom { get; set; }
		public int? TargetRoomId { get; set; }
		public Room TargetRoom { get; set; }
		public DirectionType DirectionType { get; set; }
		public string Description { get; set; }
		public string Keyword { get; set; }
		public RoomDirectionFlags Flags { get; set; }
		public int Key { get; set; }
		public int TargetRoomVNum { get; set; }
	}
}
