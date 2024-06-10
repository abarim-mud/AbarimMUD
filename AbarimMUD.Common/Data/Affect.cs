namespace AbarimMUD.Data
{
	public enum EffectType
	{
		Strength,
		Dexterity,
		Intelligence,
		Wisdom,
		Constitution,
		Sex,
		Class,
		Level,
		Age,
		Height,
		Weight,
		Mana,
		Hit,
		Move,
		Gold,
		Exp,
		Ac,
		HitRoll,
		DamRoll,
		Saves,
		SavingPara = Saves,
		SavingRod,
		SavingPetri,
		SavingBreath,
		SavingSpell,
		SpellAffect
	}

	public class Affect
	{
		public EffectType EffectType { get; set; }
		public int Modifier { get; set; }
	}
}
