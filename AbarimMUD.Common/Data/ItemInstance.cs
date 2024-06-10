namespace AbarimMUD.Data
{
	public class ItemInstance
	{
		public Item Info { get; set; }

		public int Quantity { get; set; } = 1;

		public int Id => Info.Id;

		public string[] Keywords => Info.Name.SplitByWhitespace();

		public ItemInstance()
		{
		}

		public ItemInstance(Item item)
		{
			Info = item;
		}

		public static bool AreEqual(ItemInstance a, ItemInstance b) => a.Id == b.Id;
	}
}
