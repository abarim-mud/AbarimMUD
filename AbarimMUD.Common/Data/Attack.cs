using System;

namespace AbarimMUD.Data
{
	public class Attack
	{
		private int _accuracy, _minimumDamage, _maximumDamage;

		public AttackType AttackType { get; set; }
		
		public int Accuracy
		{
			get => _accuracy;
			set
			{
				_accuracy = value;
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

		public Attack(AttackType attackType, int accuracy, int minimumDamage, int maximumDamage)
		{
			AttackType = attackType;
			Accuracy = accuracy;
			MinimumDamage = minimumDamage;
			MaximumDamage = maximumDamage;
		}

		public override string ToString()
		{
			return $"{AttackType}, {Accuracy}, {MinimumDamage} - {MaximumDamage}";
		}
	}
}
