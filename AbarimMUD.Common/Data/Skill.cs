using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class SkillLevelDefinition
	{
		public Dictionary<ModifierType, int> Modifiers { get; set; } = new Dictionary<ModifierType, int>();
		public Dictionary<ModifierType, int> PrimeModifiers { get; set; } = new Dictionary<ModifierType, int>();
		public Ability[] Abilities { get; set; }
	}

	public class Skill : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Skill> Storage = new Skills();

		public string Id { get; set; }
		public string Name { get; set; }
		public PlayerClass Class { get; set; }
		public SkillLevelDefinition[] Levels { get; set; }
		public int Cost { get; set; }

		[JsonIgnore]
		public int TotalLevels => Levels.Length;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Skill GetSkillById(string name) => Storage.GetByKey(name);
		public static Skill EnsureSkillById(string name) => Storage.EnsureByKey(name);
	}

	public class SkillValue
	{
		public Skill Skill { get; set; }
		public int Level { get; set; }

		public bool IsMaxed => Level >= Skill.TotalLevels;

		internal SkillValue()
		{
		}

		public SkillValue(Skill skill, int level)
		{
			Skill = skill ?? throw new ArgumentNullException(nameof(skill));
			Level = level;
		}

		public SkillValue(Skill skill) : this(skill, 1)
		{
		}
	}
}
