using AbarimMUD.Data;

namespace AbarimMUD
{
	public struct DamageResult
	{
		public int InitialDamage;
		public int ArmorAbsorbedDamage;

		public int Damage => InitialDamage - ArmorAbsorbedDamage;

		public DamageResult(int initialDamage, int armorAbsorbedDamage)
		{
			InitialDamage = initialDamage;
			ArmorAbsorbedDamage = armorAbsorbedDamage;
		}

		public override string ToString() => $"{InitialDamage}-{ArmorAbsorbedDamage}={Damage}";
	}

	public static class Combat
	{
		public static DamageResult CalculateDamage(Attack attack, int armorClass)
		{
			var result = new DamageResult
			{
				InitialDamage = attack.DamageRange.Generate()
			};

			if (result.InitialDamage <= 0)
			{
				return result;
			}

			var armorFactor = Utility.Clamp((100 + armorClass - attack.Penetration) / 200.0f);
			result.ArmorAbsorbedDamage = (int)(armorFactor * result.InitialDamage);

			return result;
		}
	}
}
