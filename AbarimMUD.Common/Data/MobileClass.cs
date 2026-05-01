using AbarimMUD.Attributes;
using AbarimMUD.Utils;
using System.Collections.Generic;
using Ur;

namespace AbarimMUD.Data
{
	public class MobileClassAttackInfo
	{
		public int MinimumLevel { get; set; }
		public AttackType? Type { get; set; }
		public LeveledValue AttackBonus { get; set; }
		public LeveledValue MinimumDamage { get; set; }
		public LeveledValue MaximumDamage { get; set; }
	}

	public class MobileClass : IStoredInFile
	{
		public static readonly MultipleFilesStorage<MobileClass> Storage = new MultipleFilesStorage<MobileClass>("mobileClasses");

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }

		public LeveledValue Armor { get; set; }
		public LeveledValue Gold { get; set; }
		public LeveledValue Hitpoints { get; set; }
		public LeveledValue Mana { get; set; }
		public LeveledValue Moves { get; set; }
		public List<MobileClassAttackInfo> Attacks { get; set; } = new List<MobileClassAttackInfo>();

		public LeveledValue? HolyResistance { get; set; }
		public LeveledValue? FireResistance { get; set; }
		public LeveledValue? ColdResistance { get; set; }
		public LeveledValue? ShockResistance { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static MobileClass GetClassById(string name) => Storage.GetByKey(name);
		public static MobileClass EnsureClassById(string name) => Storage.EnsureByKey(name);

	}
}
