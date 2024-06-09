namespace AbarimMUD.Data
{
	public class CreatureStats
	{
		public int MaxHitpoints { get; internal set; }
		public int MaxMana { get; internal set; }
		public int MaxMovement { get; internal set; }
		public Attack[] Attacks { get; internal set; }
		public int ArmorClass { get; internal set; }
	}
}
