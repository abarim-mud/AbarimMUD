using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class ItemInstance
	{
		public Item Info { get; set; }
		public Enchantment Enchantment { get; set; }

		[JsonIgnore]
		public string Id => Info.Id;

		[JsonIgnore]
		public int Price => Info.Price;

		public string JustName => Info.ShortDescription;

		[JsonIgnore]
		public string Name
		{
			get
			{
				if (Enchantment == null)
				{
					return JustName;
				}

				return $"{JustName} of {Enchantment.Name}";
			}
		}

		[JsonIgnore]
		public ItemType ItemType => Info.ItemType;

		[JsonIgnore]
		public SlotType? EquipmentSlot => Info.EquipmentSlot;

		public ItemInstance()
		{
		}

		public ItemInstance(Item item)
		{
			Info = item;
		}


		public bool MatchesKeyword(string keyword) => Info.MatchesKeyword(keyword);


		public override string ToString() => Info.ToString();

		public static bool AreEqual(ItemInstance a, ItemInstance b)
		{
			if (a.Id != b.Id)
			{
				return false;
			}

			if (a.Enchantment == null && b.Enchantment == null)
			{
				return true;
			}

			if (a.Enchantment == null && b.Enchantment != null)
			{
				return false;
			}

			if (a.Enchantment != null && b.Enchantment == null)
			{
				return false;
			}

			return a.Enchantment.Id == b.Enchantment.Id;
		}

		public ItemInstance Clone() => new ItemInstance
		{
			Info = Info
		};
	}
}
