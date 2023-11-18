using System;

namespace AbarimMUD.Common.Data
{
	[Flags]
	public enum RoomDirectionFlags
	{
		None = 0,
		IsDoor = 1 << 0,
		PickProof = 1 << 1,
		Hidden = 1 << 2,
	}

	public enum DirectionType
	{
		North,
		East,
		South,
		West,
		Up,
		Down,
		NorthWest,
		NorthEast,
		SouthEast,
		SouthWest
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
		public string GeneralDescription { get; set; }
		public string Keyword { get; set; }
		public RoomDirectionFlags Flags { get; set; }
		public int? Key { get; set; }
		public int? ToRoom { get; set; }
	}
}
