using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMovement { get; internal set; }
		public List<Attack> Attacks { get; } = new List<Attack>();
		public int Armor { get; internal set; }
		public int XpAward { get; internal set; }
	}
}
