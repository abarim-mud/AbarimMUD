using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Area : Entity
	{
		public string Name { get; set; }
		public string Credits { get; set; }
		public string Builders { get; set; }
		public int Security { get; set; } = 9;
		public int StartRoomVNum { get; set; }
		public int EndRoomVNum { get; set; }
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
