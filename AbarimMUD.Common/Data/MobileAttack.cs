namespace AbarimMUD.Data
{
	public class MobileAttack
	{
		public AttackType AttackType { get; set; }
		public int MinimumDamage { get; set; }
		public int MaximumDamage { get; set; }
		public int Penetration { get; set; }

		public override string ToString()
		{
			return $"{AttackType}, {Penetration}, {MinimumDamage} - {MaximumDamage}";
		}
	}
}
