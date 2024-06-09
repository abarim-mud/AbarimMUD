using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class GameClass
	{
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		public string Name { get; set; }
		public Dictionary<int, List<Skill>> Skills { get; set; }

		public static GameClass GetClassByName(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassByName(string name) => Storage.EnsureByKey(name);
		public static GameClass LookupClass(string name) => Storage.Lookup(name);
	}
}
