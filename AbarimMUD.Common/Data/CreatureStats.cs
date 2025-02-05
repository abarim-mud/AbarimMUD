using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMoves { get; internal set; }

		public int HpRegenBonus { get; internal set; }
		public int ManaRegenBonus { get; internal set; }
		public int MovesRegenBonus { get; internal set; }

		public List<Attack> Attacks { get; } = new List<Attack>();
		public int BackstabCount { get; internal set; }
		public int BackstabMultiplier { get; internal set; }
		public int Armor { get; internal set; }
		public long XpAward { get; internal set; }
		public List<Ability> Abilities { get; } = new List<Ability>();

		public int GetHitpointsRegen(bool isFighting)
		{
			var result = (int)(Configuration.HitpointsRegenPercentagePerMinute * MaxHitpoints / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += HpRegenBonus;

			return result;
		}

		public int GetManaRegen(bool isFighting)
		{
			var result = (int)(Configuration.ManaRegenPercentagePerMinute * MaxMana / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += ManaRegenBonus;

			return result;
		}

		public int GetMovesRegen(bool isFighting)
		{
			var result = (int)(Configuration.MovesRegenPercentagePerMinute * MaxMoves / 100.0f);
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += MovesRegenBonus;

			return result;
		}

		public Ability GetAbility(string id)
		{
			return (from ab in Abilities where string.Equals(ab.Id, id, System.StringComparison.InvariantCultureIgnoreCase) select ab).FirstOrDefault();
		}
	}
}
