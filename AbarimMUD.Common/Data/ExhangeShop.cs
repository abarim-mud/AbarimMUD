using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class Exchange
	{
		public ItemInstance Item { get; set; }
		public Inventory Price { get; set; } = new Inventory();
	}

	public class ExchangeShop : IStoredInFile
	{
		public static readonly MultipleFilesStorage<ExchangeShop> Storage = new ExchangeShops();

		public string Id { get; set; }
		public List<Exchange> Exchanges { get; set; } = new List<Exchange>();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static ExchangeShop GetExchangeShopById(string id) => Storage.GetByKey(id);
		public static ExchangeShop EnsureExchangeShopById(string id) => Storage.EnsureByKey(id);
	}
}
