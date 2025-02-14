using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum ModifierType
	{
		AttacksCount,
		WeaponHit,
		Hit,
		BackstabCount,
		BackstabMultiplier,
		Armor,
		HpRegen,
		ManaRegen,
		MovesRegen,
		BuyPriceRebatePercentage,
		SellPriceBonusPercentage
	}

	public class Affect
	{
		[JsonIgnore]
		public ModifierType Type { get; set; }
		public int Value { get; set; }
		public int? DurationInSeconds { get; set; }
		public string AffectSlotName { get; set; }

		public Affect()
		{
		}

		public Affect(ModifierType type, int value, int? durationInSeconds = null)
		{
			Type = type;
			Value = value;
			DurationInSeconds = durationInSeconds;
		}

		public Affect Clone() => new Affect(Type, Value, DurationInSeconds)
		{
			AffectSlotName = AffectSlotName
		};
	}
}
