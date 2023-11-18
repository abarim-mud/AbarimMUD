using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public class Room: AreaEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public int Flags { get; set; }
		public SectorType SectorType { get; set; }
		public int HealRate { get; set; }
		public int ManaRate { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
		public string Owner { get; set; }
		public ICollection<RoomDirection> OutputDirections { get; } = new List<RoomDirection>();
		public ICollection<RoomDirection> InputDirections { get; } = new List<RoomDirection>();
	}
}
