using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMoves { get; internal set; }

		/// <summary>
		/// It's internal since real regen depends on whether the creature is fighting or not
		/// Use GetHitpointsRegen method to get the real regen
		/// </summary>
		internal int HitpointsRegen { get; set; }
		internal int ManaRegen { get; set; }
		internal int MovesRegen { get; set; }

		public List<Attack> Attacks { get; } = new List<Attack>();
		public int BackstabCount { get; internal set; }
		public int BackstabMultiplier { get; internal set; }
		public int Armor { get; internal set; }
		public long XpAward { get; internal set; }

		internal void ApplyModifier(ModifierType modifier, int value)
		{
			if (value == 0)
			{
				return;
			}

			switch (modifier)
			{
				case ModifierType.BackstabCount:
					BackstabCount += value;
					break;

				case ModifierType.BackstabMultiplier:
					BackstabMultiplier += value;
					break;
			}
		}

		public int GetHitpointsRegen(bool isFighting)
		{
			var result = HitpointsRegen;

			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}

		public int GetManaRegen(bool isFighting)
		{
			var result = ManaRegen;

			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}

		public int GetMovesRegen(bool isFighting)
		{
			var result = MovesRegen;

			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}
	}
}
