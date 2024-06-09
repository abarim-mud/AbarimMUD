using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class Creature
	{
		private CreatureStats _stats = null;
		private Race _race;
		private GameClass _class;
		private int _level;

		public string Name { get; set; }
		public Race Race
		{
			get => _race;

			set
			{
				_race = value ?? throw new ArgumentNullException(nameof(value));
				InvalidateStats();
			}
		}

		public GameClass Class
		{
			get => _class;

			set
			{
				_class = value ?? throw new ArgumentNullException(nameof(value));
				InvalidateStats();
			}
		}

		public int Level
		{
			get => _level;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_level = value;
				InvalidateStats();
			}
		}

		public CreatureStats Stats
		{
			get
			{
				UpdateStats();
				return _stats;
			}
		}

		public Sex Sex { get; set; }

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
				Hitpoints = Class.HitpointsPerLevel * Level
			};

			var attacksCount = 1;
			var attacksList = new List<Attack>();

			for (var i = 0; i < attacksCount; ++i)
			{
				attacksList.Add(new Attack(AttackType.Hit, 50, new RandomRange(1, 4)));
			}

			_stats.Attacks = attacksList.ToArray();
		}
	}
}
