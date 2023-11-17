using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public class Room
	{
		public int Id { get; set; }
		public int? VNum { get; set; }
		public Zone Zone { get; set; }
		public int ZoneId { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public long Flags1 { get; set; }
		public long Flags2 { get; set; }
		public long Flags3 { get; set; }
		public long Flags4 { get; set; }
		public int SectorType { get; set; }
		public string NDKeyword { get; set; }
		public string NDDescription { get; set; }
		public ICollection<RoomDirection> OutputDirections { get; } = new List<RoomDirection>();
		public ICollection<RoomDirection> InputDirections { get; } = new List<RoomDirection>();
	}
}
