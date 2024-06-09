namespace AbarimMUD.Data
{
	public class Attack
	{
		private int _penetration;

		public AttackType AttackType { get; set; }

		public int Penetration
		{
			get => _penetration;
			set
			{
				_penetration = value;
			}
		}

		public RandomRange DamageRange;

		public Attack()
		{
		}

		public Attack(AttackType attackType, int penetration, RandomRange damageRange)
		{
			AttackType = attackType;
			Penetration = penetration;
			DamageRange = damageRange;
		}

		public Attack(AttackType attackType, int penetration, int min, int max) : this(attackType, penetration, new RandomRange(min, max))
		{
		}

		public override string ToString()
		{
			return $"{AttackType}, {Penetration}, {DamageRange}";
		}
	}
}
