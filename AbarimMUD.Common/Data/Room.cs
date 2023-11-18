using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public class Room: AreaEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public int Flags1 { get; set; }
		public int Flags2 { get; set; }
		public int Flags3 { get; set; }
		public int Flags4 { get; set; }
		public int SectorType { get; set; }
		public string NDKeyword { get; set; }
		public string NDDescription { get; set; }
		public ICollection<RoomDirection> OutputDirections { get; } = new List<RoomDirection>();
		public ICollection<RoomDirection> InputDirections { get; } = new List<RoomDirection>();
	}
}
