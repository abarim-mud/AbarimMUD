using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public abstract class Creature
	{
		public static readonly List<Creature> AllCreatures = new List<Creature>();

		private CreatureStats _stats = null;

		public abstract string ShortDescription { get; }
		public abstract string Description { get; }
		public abstract Race Race { get; }

		public abstract GameClass Class { get; }

		public abstract int Level { get; }
		public abstract Sex Sex { get; }
		public Equipment Equipment { get; set; } = new Equipment();
		public Inventory Inventory { get; set; } = new Inventory();

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
				MaxHitpoints = (int)(Race.HitpointsModifier * Class.Hitpoints.CalculateValue(Level)),
				Armor = Race.NaturalArmor.CalculateValue(Level),
			};

			var attacksCount = Race.NaturalAttacksCount.CalculateValue(Level);
			var penetration = (int)(Race.PenetrationModifier * Class.Penetration.CalculateValue(Level));
			var minimumDamage = Race.BareHandedMinimumDamage.CalculateValue(Level);
			var maximumDamage = Race.BareHandedMaximumDamage.CalculateValue(Level);

			// Apply skill modifiers
			foreach (var pair in Class.SkillsByLevels)
			{
				if (Level < pair.Key)
				{
					continue;
				}

				foreach (var skill in pair.Value)
				{
					foreach (var pair2 in skill.Modifiers)
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

			// Build attack list
			var attack = new Attack(Race.AttackType, penetration, new RandomRange(minimumDamage, maximumDamage));
			var attacksList = new List<Attack>();
			for (var i = 0; i < attacksCount; ++i)
			{
				attacksList.Add(attack);
			}

			_stats.Attacks = attacksList.ToArray();
		}

		public void Restore()
		{
			var stats = Stats;

			State.Hitpoints = stats.MaxHitpoints;
		}

		public bool? Wear(ItemInstance item)
		{
			var result = Equipment.Wear(item);

			InvalidateStats();

			return result;
		}

		public ItemInstance Remove(SlotType slot)
		{
			var result = Equipment.Remove(slot);

			InvalidateStats();

			return result;
		}

		public abstract bool MatchesKeyword(string keyword);
	}
}
