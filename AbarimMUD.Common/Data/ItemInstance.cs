using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class ItemInstance
	{
		public Item Info { get; set; }

		public string Id => Info.Id;

		public int Price => Info.Price;

		[JsonIgnore]
		public string ShortDescription => Info.ShortDescription;

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

		public static bool AreEqual(ItemInstance a, ItemInstance b) => a.Id == b.Id;
	}
}
