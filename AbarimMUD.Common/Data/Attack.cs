using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public class Attack
	{
		public AttackType AttackType { get; set; }

		public int Penetration { get; set; }

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
			Penetration = penetration;
			DamageRange = damageRange;
		}

		public Attack(AttackType attackType, int penetration, int min, int max) : this(attackType, penetration, new ValueRange(min, max))
		{
		}

		public Attack Clone() => new Attack(AttackType, Penetration, DamageRange);

		public override string ToString()
		{
			return $"{AttackType}, {Penetration}, {DamageRange}";
		}
	}
}
