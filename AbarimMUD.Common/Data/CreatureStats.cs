using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int HitpointsBase { get; internal set; }
		public int ManaBase { get; internal set; }
		public int MovesBase { get; internal set; }

		public int HitpointsBonus { get; internal set; }
		public int ManaBonus { get; internal set; }
		public int MovesBonus { get; internal set; }


		public int MaxHitpoints => HitpointsBase + HitpointsBonus;
		public int MaxMana => ManaBase + ManaBonus;
		public int MaxMoves => MovesBase + MovesBonus;

		public int HpRegenPercentage { get; internal set; }
		public int ManaRegenPercentage { get; internal set; }
		public int MovesRegenPercentage { get; internal set; }

		public int HpRegenAbsolute { get; internal set; }
		public int ManaRegenAbsolute { get; internal set; }
		public int MovesRegenAbsolute { get; internal set; }

		public int HpRegenAbsolute2 { get; internal set; }
		public int ManaRegenAbsolute2 { get; internal set; }
		public int MovesRegenAbsolute2 { get; internal set; }

		public List<Attack> Attacks { get; } = new List<Attack>();
		public int BackstabCount { get; internal set; }
		public int BackstabMultiplier { get; internal set; }
		public int Armor { get; internal set; }
		public List<Ability> Abilities { get; } = new List<Ability>();
		public int BuyPriceRebatePercentage { get; internal set; }
		public int SellPriceBonusPercentage { get; internal set; }
		public int DamageReduction { get; internal set; }
		public int DeathtouchMultiplier { get; internal set; }

		public int GetHitpointsRegen(bool isFighting)
		{
			var r = (int)((Configuration.HitpointsRegenPercentagePerMinute + HpRegenPercentage) * HitpointsBase / 100.0f);
			var result = Math.Max(r, Configuration.HitpointsRegenMinimumPerMinute) + HpRegenAbsolute;
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += HpRegenAbsolute2;

			return result;
		}

		public int GetManaRegen(bool isFighting)
		{
			var r = (int)((Configuration.ManaRegenPercentagePerMinute + ManaRegenPercentage) * ManaBase / 100.0f);
			var result = Math.Max(r, Configuration.ManaRegenMinimumPerMinute) + ManaRegenAbsolute;
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += ManaRegenAbsolute2;

			return result;
		}

		public int GetMovesRegen(bool isFighting)
		{
			var r = (int)((Configuration.MovesRegenPercentagePerMinute + MovesRegenPercentage) * MaxMoves / 100.0f);
			var result = Math.Max(r, Configuration.MovesRegenMinimumPerMinute) + MovesRegenAbsolute;
			if (!isFighting)
			{
				result *= Configuration.PeaceRegenMultiplier;
			}

			result += MovesRegenAbsolute2;

			return result;
		}

		public Ability GetAbility(string id)
		{
			return (from ab in Abilities where ab.Id.EqualsToIgnoreCase(id) select ab).FirstOrDefault();
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
			return 1.0f + Math.Min(value, 300) / 100.0f;
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
				k = CalculateArmorPenK(attack.AttackBonus);

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

		public void Apply(ModifierType type, int val, bool usesWeapon)
		{
			switch (type)
			{
				case ModifierType.Hitpoints:
					HitpointsBonus += val;
					break;
				case ModifierType.Mana:
					ManaBonus += val;
					break;
				case ModifierType.Moves:
					MovesBonus += val;
					break;
				case ModifierType.AttackBonus:
					foreach (var atk in Attacks)
					{
						atk.AttackBonus += val;
					}
					break;
				case ModifierType.WeaponAttackBonus:
					foreach (var atk in Attacks)
					{
						if (usesWeapon)
						{
							atk.AttackBonus += val;
						}
					}
					break;
				case ModifierType.MartialArtsAttackBonus:
					foreach (var atk in Attacks)
					{
						if (!usesWeapon)
						{
							atk.AttackBonus += val;
						}
					}
					break;
				case ModifierType.Damage:
					foreach (var atk in Attacks)
					{
						atk.DamageRange += val;
					}
					break;
				case ModifierType.WeaponDamage:
					foreach (var atk in Attacks)
					{
						if (usesWeapon)
						{
							atk.DamageRange += val;
						}
					}
					break;
				case ModifierType.MartialArtsDamage:
					foreach (var atk in Attacks)
					{
						if (!usesWeapon)
						{
							atk.DamageRange += val;
						}
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
				case ModifierType.HpRegenPercentage:
					HpRegenPercentage += val;
					break;
				case ModifierType.ManaRegenPercentage:
					ManaRegenPercentage += val;
					break;
				case ModifierType.MovesRegenPercentage:
					MovesRegenPercentage += val;
					break;
				case ModifierType.HpRegenAbsolute:
					HpRegenAbsolute += val;
					break;
				case ModifierType.ManaRegenAbsolute:
					ManaRegenAbsolute += val;
					break;
				case ModifierType.MovesRegenAbsolute:
					MovesRegenAbsolute += val;
					break;
				case ModifierType.HpRegenAbsolute2:
					HpRegenAbsolute2 += val;
					break;
				case ModifierType.ManaRegenAbsolute2:
					ManaRegenAbsolute2 += val;
					break;
				case ModifierType.MovesRegenAbsolute2:
					MovesRegenAbsolute2 += val;
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
				case ModifierType.MartialArtsMinimumDamage:
					foreach (var atk in Attacks)
					{
						if (!usesWeapon)
						{
							atk.DamageRange.Minimum = Math.Max(atk.DamageRange.Minimum, val);
						}
					}
					break;
				case ModifierType.MartialArtsMaximumDamage:
					foreach (var atk in Attacks)
					{
						if (!usesWeapon)
						{
							atk.DamageRange.Maximum = Math.Max(atk.DamageRange.Maximum, val);
						}
					}
					break;
				case ModifierType.DamageReduction:
					DamageReduction += val;
					break;
				case ModifierType.DeathtouchMultiplier:
					DeathtouchMultiplier += val;
					break;
			}
		}
	}
}