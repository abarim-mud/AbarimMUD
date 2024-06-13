using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public struct RaceClassValueRange
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

	public class GameClass: IEntity
	{
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		public string Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }

		[JsonConverter(typeof(Common.GameClassConverter))]
		public GameClass Inherits { get; set; }
		
		public RaceClassValueRange Hitpoints;
		
		public RaceClassValueRange Penetration;

		public bool IsPlayerClass { get; set; }

		public Dictionary<int, List<Skill>> SkillsByLevels { get; set; } = new Dictionary<int, List<Skill>>();

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static GameClass GetClassById(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassById(string name) => Storage.EnsureByKey(name);
	}
}
