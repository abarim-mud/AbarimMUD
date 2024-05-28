using System;
using System.Collections.Generic;
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
		WaterNoSwim,
		Unused,
		Air,
		Desert,
		River,
		Cave,
		Swim,
		Swamp,
		Underground,
		Trail,
		Road,
		Ocean
	}

	public enum RoomFlags
	{
		NoMob,
		Dark,
		Indoors,
		Law,
		Private,
		NoRecall,
		Solitary,
		HeroesOnly,
		GodsOnly,
		ImpOnly,
		Safe,
		PetShop,
		NewbiesOnly,
		Nowhere,
	}

	public class Room: AreaEntity
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public HashSet<RoomFlags> Flags { get; set; }
		public SectorType SectorType { get; set; }
		public int HealRate { get; set; }
		public int ManaRate { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
		public string Owner { get; set; }

		public Dictionary<Direction, RoomExit> Exits { get; set; }

		[JsonIgnore]
		public List<MobileInstance> Mobiles { get; } = new List<MobileInstance>();

		[JsonIgnore]
		public List<Character> Characters { get; } = new List<Character>();

		public void InitializeLists()
		{
			Exits = new Dictionary<Direction, RoomExit>();
		}

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
