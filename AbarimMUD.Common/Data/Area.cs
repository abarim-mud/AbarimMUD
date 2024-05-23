using System.Collections.Generic;
using System.Text.Json.Serialization;

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
		[JsonIgnore]
		public string Name
		{
			get => Id;
			set => Id = value;
		}

		public string Credits { get; set; }
		public string Builders { get; set; }
		public int? MinimumLevel { get; set; }
		public int? MaximumLevel { get; set; }
		public List<Room> Rooms { get; set; } = new List<Room>();

		[JsonIgnore]
		public List<Mobile> Mobiles { get; } = new List<Mobile>();

		[JsonIgnore]
		public List<GameObject> Objects { get; } = new List<GameObject>();

		[JsonIgnore]
		public List<AreaReset> Resets { get; } = new List<AreaReset>();

		public override string ToString() => $"{Name}";
	}
}
