﻿using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum ModifierType
	{
		Hitpoints,
		Mana,
		Moves,
		AttacksCount,
		AttackBonus,
		Damage,
		WeaponAttacksCount,
		WeaponAttackBonus,
		WeaponDamage,
		BackstabCount,
		BackstabMultiplier,
		Armor,
		HpRegenPercentage,
		ManaRegenPercentage,
		MovesRegenPercentage,
		HpRegenAbsolute,
		ManaRegenAbsolute,
		MovesRegenAbsolute,
		HpRegenAbsolute2,
		ManaRegenAbsolute2,
		MovesRegenAbsolute2,
		BuyPriceRebatePercentage,
		SellPriceBonusPercentage,
		MartialArtsMinimumDamage,
		MartialArtsMaximumDamage,
		MartialArtsAttacksCount,
		MartialArtsAttackBonus,
		MartialArtsDamage,
		DamageReduction,
		DeathtouchMultiplier
	}

	public class Affect
	{
		[JsonIgnore]
		public ModifierType Type { get; set; }
		public int Value { get; set; }
		public int? DurationInSeconds { get; set; }
		public string AffectSlotName { get; set; }

		public Affect()
		{
		}

		public Affect(ModifierType type, int value, int? durationInSeconds = null)
		{
			Type = type;
			Value = value;
			DurationInSeconds = durationInSeconds;
		}

		public Affect Clone() => new Affect(Type, Value, DurationInSeconds)
		{
			AffectSlotName = AffectSlotName
		};
	}
}
