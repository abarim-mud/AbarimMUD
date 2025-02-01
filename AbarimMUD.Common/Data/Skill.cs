﻿using AbarimMUD.Attributes;
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

	public enum ModifierType
	{
		AttacksCount,
		Penetration,
		BackstabCount,
		BackstabMultiplier
	}

	public class SkillLevelDefinition
	{
		public Dictionary<ModifierType, int> Modifiers { get; set; } = new Dictionary<ModifierType, int>();
	}

	public class Skill : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Skill> Storage = new MultipleFilesStorage<Skill>(r => r.Id, "skills");

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public SkillLevelDefinition[] Levels { get; set; }

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
