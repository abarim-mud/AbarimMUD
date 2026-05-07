using System;

namespace AbarimMUD.Data
{
	public class TemporaryAffect
	{
		public AbilityPower AbilityPower { get; set; }
		public string Name { get; }
		public ModifierType Type { get; }
		public int Value { get; }
		public int DurationInSeconds { get; }
		public DateTime Started { get; }
		public bool WarnedAboutToExpire { get; set; }

		public TemporaryAffect(AbilityPower abilityPower, string name, ModifierType type, int value, int durationInSeconds)
		{
			if (abilityPower == null)
			{
				throw new ArgumentNullException("source");
			}

			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			AbilityPower = abilityPower;
			Name = name;
			Type = type;
			Value = value;
			DurationInSeconds = durationInSeconds;
			Started = DateTime.Now;
		}
	}
}
