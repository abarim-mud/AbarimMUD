namespace AbarimMUD.Data
{
	public class MobileSpecialAttack
	{
		public string AttackType { get; set; }

		public MobileSpecialAttack Clone() => new MobileSpecialAttack
		{
			AttackType = AttackType
		};
	}
}
