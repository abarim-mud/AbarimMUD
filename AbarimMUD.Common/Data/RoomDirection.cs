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

	public class RoomDirection
	{
		public int Id { get; set; }
		public int RoomId { get; set; }
		public string GeneralDescription { get; set; }
		public string Keyword { get; set; }
		public RoomDirectionFlags Flags { get; set; }
		public int? Key { get; set; }
		public int? ToRoom { get; set; }
	}
}
