using System;
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
		public List<Ability> Abilities { get; } = new List<Ability>();
		public int BuyPriceRebatePercentage { get; internal set; }
		public int SellPriceBonusPercentage { get; internal set; }

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

		public int GetBuyPrice(int originalPrice)
		{
			var perc = Configuration.BaseBuyPricePercentage;

			perc -= BuyPriceRebatePercentage;

			return originalPrice * perc / 100;
		}

		public int GetSellPrice(int originalPrice)
		{
			var perc = Configuration.BaseSellPricePercentage;

			perc += SellPriceBonusPercentage;

			return originalPrice * perc / 100;
		}

		private static float CalculateArmorPenK(int value)
		{
			return Math.Max(1, Math.Min(value, 300) / 100.0f);
		}


		public long CalculateXpAward()
		{
			long xpAward = Configuration.XpMultiply;
			
			xpAward *= Math.Max(1, MaxHitpoints);

			var k = CalculateArmorPenK(Armor);
			xpAward = (int)(xpAward * k);

			long attackXpFactor = 0;
			foreach (var attack in Attacks)
			{
				k = CalculateArmorPenK(attack.Penetration);

				var t = (long)(Math.Max(1, attack.AverageDamage) * k);

				attackXpFactor += t;
			}

			xpAward *= attackXpFactor;

			if (xpAward < 1)
			{
				xpAward = 1;
			}

			return xpAward;
		}

		public void Apply(ModifierType type, int val)
		{
			switch (type)
			{
				case ModifierType.WeaponPenetration:
					foreach (var atk in Attacks)
					{
						atk.Penetration += val;
					}
					break;
				case ModifierType.BackstabCount:
					BackstabCount += val;
					break;
				case ModifierType.BackstabMultiplier:
					BackstabMultiplier += val;
					break;
				case ModifierType.Armor:
					Armor += val;
					break;
				case ModifierType.HpRegen:
					HpRegenBonus += val;
					break;
				case ModifierType.ManaRegen:
					ManaRegenBonus += val;
					break;
				case ModifierType.MovesRegen:
					MovesRegenBonus += val;
					break;
				case ModifierType.AttacksCount:
					// This is calculated on earlier stage of Creature.UpdateStats
					// Hence ignoring it here
					break;
				case ModifierType.BuyPriceRebatePercentage:
					BuyPriceRebatePercentage += val;
					break;
				case ModifierType.SellPriceBonusPercentage:
					SellPriceBonusPercentage += val;
					break;
			}
		}
	}
}
