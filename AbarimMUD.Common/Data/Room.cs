using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
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

	[Flags]
	public enum RoomFlags
	{
		None = 0,
		Dark = 1 << 0,
		NoMob = 1 << 2,
		InDoors = 1 << 3,
		Private = 1 << 9,
		Safe = 1 << 10,
		Solitary = 1 << 11,
		PetShop = 1 << 12,
		NoRecall = 1 << 13,
		ImpOnly = 1 << 14,
		GodsOnly = 1 << 15,
		HeroesOnly = 1 << 16,
		NewbiesOnly = 1 << 17,
		Law = 1 << 18,
		Nowhere = 1 << 19
	}

	public class Room: AreaEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public RoomFlags Flags { get; set; }
		public SectorType SectorType { get; set; }
		public int HealRate { get; set; }
		public int ManaRate { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
		public string Owner { get; set; }

		public Dictionary<Direction, RoomExit> Exits { get; set; } = new Dictionary<Direction, RoomExit>();

		[JsonIgnore]
		public List<MobileInstance> Mobiles { get; } = new List<MobileInstance>();

		[JsonIgnore]
		public List<Character> Characters { get; } = new List<Character>();

		public void AddCharacter(Character character)
		{
			// Remove all characters with such id
			Characters.RemoveAll(character1 => character1.Name == character.Name);
			Characters.Add(character);
		}

		public void RemoveCharacter(Character character)
		{
			Characters.Remove(character);
		}

		public override string ToString() => $"{Name} (#{Id})";
	}
}
