using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ModifierType
	{
		AttacksCount
	}

	public class Skill
	{
		public static readonly MultipleFilesStorageString<Skill> Storage = new MultipleFilesStorageString<Skill>(r => r.Name, "skills");

		public string Name { get; set; }
		public Dictionary<ModifierType, int> Modifiers { get; set; }

		public static Skill GetSkillByName(string name) => Storage.GetByKey(name);
		public static Skill EnsureSkillByName(string name) => Storage.EnsureByKey(name);
		public static Skill LookupSkill(string name) => Storage.Lookup(name);
	}
}
