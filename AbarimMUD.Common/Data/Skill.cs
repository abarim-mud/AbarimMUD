using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ModifierType
	{
		BackstabCount
	}

	public class Skill : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Skill> Storage = new MultipleFilesStorage<Skill>(r => r.Id, "skills");

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public Dictionary<ModifierType, int> Modifiers { get; set; } = new Dictionary<ModifierType, int>();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Skill GetSkillById(string name) => Storage.GetByKey(name);
		public static Skill EnsureSkillById(string name) => Storage.EnsureByKey(name);
	}
}
