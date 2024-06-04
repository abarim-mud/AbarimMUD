namespace AbarimMUD.Data
{
	public class GameObjectEffect
	{
		public GameObject GameObject { get; set; }

		public EffectBitType EffectBitType { get; set; }
		public EffectType EffectType { get; set; }
		public int Modifier { get; set; }
		public AffectedByFlags Bits { get; set; }
	}
}
