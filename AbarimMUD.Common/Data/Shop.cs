using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class Shop : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Shop> Storage = new Shops();

		public string Id { get; set; }
		public Inventory Inventory { get; set; } = new Inventory();
		public HashSet<ItemType> ItemTypes { get; set; } = new HashSet<ItemType>();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Shop GetShopById(string id) => Storage.GetByKey(id);
		public static Shop EnsureShopById(string id) => Storage.EnsureByKey(id);
	}
}
