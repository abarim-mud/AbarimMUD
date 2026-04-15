using System;

namespace AbarimMUD.Data
{
	public class TemporaryAffect
	{
		public string Name { get; }
		public ModifierType Type { get; }
		public int Value { get; }
		public int DurationInSeconds { get; }
		public DateTime Started { get; }
		public bool WarnedAboutToExpire { get; set; }
		public string MessageDeactivated { get; set; }


		public TemporaryAffect(string name, ModifierType type, int value, int durationInSeconds, string expirationMessage)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			Name = name;
			Type = type;
			Value = value;
			DurationInSeconds = durationInSeconds;
			MessageDeactivated = expirationMessage;
			Started = DateTime.Now;
		}
	}
}
