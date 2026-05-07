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
				throw new ArgumentNullException("source");
			}

			AbilityPower = abilityPower;
			Started = DateTime.Now;
		}
	}
}
