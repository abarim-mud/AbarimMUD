using System;

namespace AbarimMUD.Data
{
	public class TemporaryAffect
	{
		public AbilityPower AbilityPower { get; set; }
		public DateTime Started { get; }
		public bool WarnedAboutToExpire { get; set; }

		public TemporaryAffect(AbilityPower abilityPower)
		{
			if (abilityPower == null)
			{
				throw new ArgumentNullException(nameof(abilityPower));
			}

			if (abilityPower.Ability == null)
			{
				throw new ArgumentNullException("abilityPower.Ability");
			}

			if (abilityPower.Ability.Affects == null)
			{
				throw new ArgumentNullException("abilityPower.Ability.Affects");
			}

			AbilityPower = abilityPower;
			Started = DateTime.Now;
		}
	}
}
