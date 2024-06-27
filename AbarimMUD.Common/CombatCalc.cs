using AbarimMUD.Data;
using System;

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

		public static DamageResult operator +(DamageResult a, DamageResult b)
		{
			return new DamageResult(a.InitialDamage + b.InitialDamage, a.ArmorAbsorbedDamage + b.ArmorAbsorbedDamage);
		}

		public static DamageResult operator -(DamageResult a, DamageResult b)
		{
			return new DamageResult(a.InitialDamage - b.InitialDamage, a.ArmorAbsorbedDamage - b.ArmorAbsorbedDamage);
		}

	}

	public static class CombatCalc
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

		public static int BackstabMult(int level)
		{
			level = Math.Min(level, 40);

			return 2 + level / 5;
		}

		public static int BackstabMovesCost() => 20;
	}
}
