using AbarimMUD.Utils;

namespace AbarimMUD.Data
{
	public enum InstantEffectType
	{
		Heal,
		MagicDamage
	}

	public class InstantEffect
	{
		public InstantEffectType Type { get; set; }
		public ValueRange Power { get; set; }
	}

	public static class InstantEffectExtensions
	{
		public static bool DoesDamage(this InstantEffectType type)
		{
			return type != InstantEffectType.Heal;
		}
	}
}
