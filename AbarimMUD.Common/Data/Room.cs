﻿using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public enum SectorType
	{
		Inside,
		City,
		Field,
		Forest,
		Hills,
		Mountain,
		WaterSwim,
		WaterNoSwim,
		Unused,
		Air,
		Desert
	}

	public class Room: AreaEntity
	{
		public int? VNum { get; set; }
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
