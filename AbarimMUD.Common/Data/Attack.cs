using System;

namespace AbarimMUD.Data
{
	public class Attack
	{
		private int _penetration, _minimumDamage, _maximumDamage;

		public AttackType AttackType { get; set; }
		public int Penetration
		{
			get => _penetration;
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_penetration = value;
			}
		}

		public int MinimumDamage
		{
			get => _minimumDamage;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_minimumDamage = value;
			}
		}
		
		public int MaximumDamage
		{
			get => _maximumDamage;
			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_maximumDamage = value;
			}
		}
		
		public Attack()
		{
		}

		public Attack(AttackType attackType, int penetration, int minimumDamage, int maximumDamage)
		{
			AttackType = attackType;
			Penetration = penetration;
			MinimumDamage = minimumDamage;
			MaximumDamage = maximumDamage;
		}

		public override string ToString()
		{
			return $"{AttackType}, {Penetration}, {MinimumDamage} - {MaximumDamage}";
		}
	}
}
