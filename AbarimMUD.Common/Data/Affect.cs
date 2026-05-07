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
		MartialArtsMinimumDamage,
		MartialArtsMaximumDamage,
		MartialArtsAttacksCount,
		MartialArtsAttackBonus,
		MartialArtsDamage,
		PhysicalResistance,
		MagicResistance,
		HolyResistance,
		FireResistance,
		ColdResistance,
		ShockResistance,
		HealBonus,
		SpellBonus
	}

	public class Affect
	{
		public ModifierType Type { get; set; }
		public int? Value { get; set; }

		public Affect()
		{
		}

		public Affect(ModifierType type, int? value)
		{
			Type = type;
			Value = value;
		}

		public Affect Clone() => new Affect(Type, Value);
	}
}
