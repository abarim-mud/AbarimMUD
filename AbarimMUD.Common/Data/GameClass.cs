using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class GameClass
	{
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		public string Name { get; set; }
		public string Description { get; set; }
		public int HitpointsPerLevel { get; set; }

		public Dictionary<int, List<Skill>> SkillsByLevels { get; set; }

		public override string ToString() => Name;

		public static GameClass GetClassByName(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassByName(string name) => Storage.EnsureByKey(name);
		public static GameClass LookupClass(string name) => Storage.Lookup(name);
	}
}
