using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMoves { get; internal set; }

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
			var result = (int)(Configuration.HitpointsRegenPercentagePerMinute * MaxHitpoints / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}

		public int GetManaRegen(bool isFighting)
		{
			var result = (int)(Configuration.ManaRegenPercentagePerMinute * MaxMana / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}

		public int GetMovesRegen(bool isFighting)
		{
			var result = (int)(Configuration.MovesRegenPercentagePerMinute * MaxMoves / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			return result;
		}
	}
}
