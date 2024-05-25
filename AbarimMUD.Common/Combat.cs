using AbarimMUD.Data;
using System;

namespace AbarimMUD
{
	public static class Combat
	{
		public static int CalculateDamage(Attack attack, int armorClass)
		{
			var damage = Utility.RandomRange(attack.MinimumDamage, attack.MaximumDamage);

			if (damage <= 0)
			{
				return 0;
			}

			var armorWithPenetration = (int)(armorClass * (1.0f - attack.Penetration / 100.0f));
			damage = Math.Max(damage - armorWithPenetration, 0);

			return damage;
		}
	}
}
