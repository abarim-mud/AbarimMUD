namespace AbarimMUD.Common.Data
{
	public class GameObjectEffect: Entity
	{
		public int GameObjectId { get; set; }
		public GameObject GameObject { get; set; }

		public EffectType EffectType { get; set; }
		public int Modifier { get; set; }
	}
}
