using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMovement { get; internal set; }
		public int HitpointsRegen { get; internal set; }
		public List<Attack> Attacks { get; } = new List<Attack>();
		public int BackstabCount { get; internal set; }
		public int Armor { get; internal set; }
		public long XpAward { get; internal set; }

		public void ApplyModifier(ModifierType modifier, int value)
		{
			if (value == 0)
			{
				return;
			}

			switch (modifier)
			{
				case ModifierType.BackstabCount:
					BackstabCount += value;
					break;
			}
		}
	}
}
