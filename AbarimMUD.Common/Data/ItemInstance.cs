using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class ItemInstance
	{
		public Item Info { get; set; }
		public Enchantement Enchantement { get; set; }

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
				if (Enchantement == null)
				{
					return JustName;
				}

				return $"{JustName} of {Enchantement.Name}";
			}
		}

		[JsonIgnore]
		public ItemType ItemType => Info.ItemType;

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

			if (a.Enchantement == null && b.Enchantement != null)
			{
				return false;
			}

			if (a.Enchantement != null && b.Enchantement == null)
			{
				return false;
			}

			return a.Enchantement.Id == b.Enchantement.Id;
		}

		public ItemInstance Clone() => new ItemInstance
		{
			Info = Info
		};
	}
}
