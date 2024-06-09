using AbarimMUD.Storage;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public struct ClassValueRange
	{
		public int Level1Value;
		public int Level100Value;

		public int CalculateValue(int level)
		{
			if (level < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			if (level == 1)
			{
				return Level1Value;
			}

			if (level == 100)
			{
				return Level100Value;
			}

			// Sqrt interpolation
			var k = (float)Math.Sqrt((level - 1) / 99.0f);
			var value = Level1Value + k * (Level100Value - Level1Value);

			return (int)value;
		}
	}

	public class GameClass
	{
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		public string Name { get; set; }
		public string Description { get; set; }
		
		public ClassValueRange Hitpoints;
		
		public ClassValueRange Penetration;
		
		public Dictionary<int, List<Skill>> SkillsByLevels { get; set; }

		public override string ToString() => Name;

		public static GameClass GetClassByName(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassByName(string name) => Storage.EnsureByKey(name);
		public static GameClass LookupClass(string name) => Storage.Lookup(name);
	}
}
