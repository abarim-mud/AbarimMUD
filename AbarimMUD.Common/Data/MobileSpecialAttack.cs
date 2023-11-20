namespace AbarimMUD.Data
{
	public class MobileSpecialAttack: Entity
	{
		public int MobileId { get; set; }
		public Mobile Mobile { get; set; }
		public string AttackType { get; set; }
	}
}
