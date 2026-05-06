using AbarimMUD.Storage;
using System.Collections.Generic;
using Ur;

namespace AbarimMUD.Data
{
	public class Shop : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Shop> Storage = new Shops();

		public string Id { get; set; }

		/// <summary>
		/// Gets ot sets the collection of items that the shop has an unlimited quantity of.
		/// </summary>
		public List<ItemInstance> UnlimitedItems { get; set; } = new List<ItemInstance>();

		/// <summary>
		/// Gets or sets the collection of limited items available in the inventory.
		/// </summary>
		public Inventory LimitedItems { get; set; } = new Inventory();

		public HashSet<ItemType> ItemTypes { get; set; } = new HashSet<ItemType>();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Shop GetShopById(string id) => Storage.GetByKey(id);
		public static Shop EnsureShopById(string id) => Storage.EnsureByKey(id);
	}
}
