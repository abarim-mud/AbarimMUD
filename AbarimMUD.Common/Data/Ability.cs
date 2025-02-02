using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum AbilityType
	{
		Passive,
		Physical
	}

	public class Ability: IStoredInFile
	{
		public static readonly MultipleFilesStorage<Ability> Storage = new Abilities();

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public AbilityType Type { get; set; }
		public Dictionary<ModifierType, int> Modifiers { get; set; }
		public PlayerClass PrimeClass { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Ability GetAbilityById(string name) => Storage.GetByKey(name);
		public static Ability EnsureAbilityById(string name) => Storage.EnsureByKey(name);
	}
}
