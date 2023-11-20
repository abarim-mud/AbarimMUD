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
		public List<Room> Rooms { get; }
		public List<Mobile> Mobiles { get; }
		public List<GameObject> Objects { get; }
		public List<AreaReset> Resets { get; }
	}
}
