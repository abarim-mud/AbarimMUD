using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public abstract class Creature
	{
		public static readonly List<Creature> AllCreatures = new List<Creature>();

		private CreatureStats _stats = null;

		public abstract string Name { get; }
		public abstract Race Race { get; }

		public abstract GameClass Class { get; }

		public abstract int Level { get; }
		public abstract Sex Sex { get; }

		public CreatureStats Stats
		{
			get
			{
				UpdateStats();
				return _stats;
			}
		}

		public CreatureState State { get; } = new CreatureState();

		public void InvalidateStats()
		{
			_stats = null;
		}

		private void UpdateStats()
		{
			if (_stats != null)
			{
				return;
			}

			_stats = new CreatureStats
			{
				MaxHitpoints = Class.HitpointsPerLevel * Level
			};

			var attacksCount = 1;
			foreach(var pair in Class.SkillsByLevels)
			{
				if (Level < pair.Key)
				{
					continue;
				}

				foreach(var skill in pair.Value)
				{
					foreach(var pair2 in skill.Modifiers)
					{
						switch (pair2.Key)
						{
							case ModifierType.AttacksCount:
								attacksCount += pair2.Value;
								break;
						}
					}
				}
			}

			var attacksList = new List<Attack>();

			for (var i = 0; i < attacksCount; ++i)
			{
				attacksList.Add(new Attack(AttackType.Hit, 50, new RandomRange(1, 4)));
			}

			_stats.Attacks = attacksList.ToArray();
		}

		public void Restore()
		{
			var stats = Stats;

			State.Hitpoints = stats.MaxHitpoints;
		}
	}
}
