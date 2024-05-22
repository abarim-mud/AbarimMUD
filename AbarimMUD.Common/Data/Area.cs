using System.Collections.Generic;

namespace AbarimMUD.Data
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
		public int? MinimumLevel { get; set; }
		public int? MaximumLevel { get; set; }
		public List<Room> Rooms { get; } = new List<Room>();
		public List<Mobile> Mobiles { get; } = new List<Mobile>();
		public List<GameObject> Objects { get; } = new List<GameObject>();
		public List<AreaReset> Resets { get; } = new List<AreaReset>();

		public override string ToString() => $"{Name}";
	}
}
