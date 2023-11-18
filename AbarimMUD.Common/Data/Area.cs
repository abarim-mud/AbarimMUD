using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Area: Entity
	{
		public string Builder { get; set; }
		public string Name { get; set; }
		public int StartRoomVNum { get; set; }
		public int MaximumRooms { get; set; }
		public int ResetInMinutes { get; set; }
		public ResetMode ResetMode { get; set; }
		public int? MinimumLevel { get; set; }
		public int? MaximumLevel { get; set; }
		public int Flags1 { get; set; }
		public int Flags2 { get; set; }
		public int Flags3 { get; set; }
		public int Flags4 { get; set; }
		public ICollection<Room> Rooms { get; }
		public ICollection<Mobile> Mobiles { get; }
		public ICollection<GameObject> Objects { get; }
	}
}
