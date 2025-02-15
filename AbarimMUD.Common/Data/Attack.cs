using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public struct AttackRollResult
	{
		public bool Hit;
		public int AttackDice;
		public int AttackRoll;

		public override string ToString()
		{
			if (AttackDice <= 10)
			{
				return "miss";
			}

			if (AttackDice >= 190)
			{
				return "hit";
			}

			return $"{AttackRoll}";
		}
	}

	public class Attack
	{
		public AttackType Type { get; set; }

		public int Bonus { get; set; }

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

		public Attack(AttackType type, int rating, ValueRange damageRange)
		{
			Type = type;
			Bonus = rating;
			DamageRange = damageRange;
		}

		public Attack(AttackType attackType, int penetration, int min, int max) : this(attackType, penetration, new ValueRange(min, max))
		{
		}

		public Attack Clone() => new Attack(Type, Bonus, DamageRange);

		public AttackRollResult DoAttackRoll(int armorClass)
		{
			var result = new AttackRollResult
			{
				AttackDice = Utility.RandomRange(1, 200)
			};

			if (result.AttackDice <= 10)
			{
				// Always miss
			}
			else if (result.AttackDice >= 190)
			{
				// Always hit
				result.Hit = true;
			}
			else
			{
				//
				result.AttackRoll = result.AttackDice + Bonus - 100;
				result.Hit = result.AttackRoll >= armorClass;
			}

			return result;
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
			return $"{Type}, {Bonus}, {DamageRange}";
		}
	}
}
