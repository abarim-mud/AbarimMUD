using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public class Attack
	{
		public AttackType AttackType { get; set; }

		public int Hit { get; set; }

		public ValueRange DamageRange;

		public int MinimumDamage
		{
			get => DamageRange.Minimum;

			set => DamageRange.Minimum = value;
		}

		public int MaximumDamage
		{
			get => DamageRange.Maximum;

			set => DamageRange.Maximum = value;
		}

		public int AverageDamage => MinimumDamage + (MaximumDamage - MinimumDamage) / 2;

		public Attack(AttackType attackType, int penetration, ValueRange damageRange)
		{
			AttackType = attackType;
			Hit = penetration;
			DamageRange = damageRange;
		}

		public Attack(AttackType attackType, int penetration, int min, int max) : this(attackType, penetration, new ValueRange(min, max))
		{
		}

		public Attack Clone() => new Attack(AttackType, Hit, DamageRange);

		public bool HitOrMiss(int armorClass, out float attackRoll)
		{
			var roll = Utility.RandomRange(1, 20);
			attackRoll = roll + (float)Hit / 10.0f;
			if (roll == 1)
			{
				return false;
			}

			if (roll == 20)
			{
				return true;
			}

			return attackRoll - 10 >= armorClass / 10.0f;
		}

		public int CalculateDamage(int damageReduction = 0)
		{
			var result = DamageRange.Random();

			var reduction = result * damageReduction / 100.0f;

			result -= (int)reduction;

			if (result < 1)
			{
				result = 1;
			}

			return result;
		}

		public override string ToString()
		{
			return $"{AttackType}, {Hit}, {DamageRange}";
		}
	}
}
