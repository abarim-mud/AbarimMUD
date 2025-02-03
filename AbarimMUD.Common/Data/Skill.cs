using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum SkillLevel
	{
		Novice,
		Apprentice,
		Adept,
		Expert,
		Master
	}

	public class SkillLevelDefinition
	{
		public Dictionary<ModifierType, int> Modifiers { get; set; } = new Dictionary<ModifierType, int>();
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

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Skill GetSkillById(string name) => Storage.GetByKey(name);
		public static Skill EnsureSkillById(string name) => Storage.EnsureByKey(name);
	}

	public class SkillValue
	{
		public Skill Skill { get; set; }
		public SkillLevel Level { get; set; }

		internal SkillValue()
		{
		}

		public SkillValue(Skill skill, SkillLevel level)
		{
			Skill = skill ?? throw new ArgumentNullException(nameof(skill));
			Level = level;
		}

		public SkillValue(Skill skill): this(skill, SkillLevel.Novice)
		{
		}
	}
}
