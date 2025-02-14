using AbarimMUD.Storage;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class ForgeShop : IStoredInFile
	{
		public static readonly MultipleFilesStorage<ForgeShop> Storage = new ForgeShops();

		public string Id { get; set; }
		public List<Forge> Forges { get; set; } = new List<Forge>();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static ForgeShop GetForgeShopById(string id) => Storage.GetByKey(id);
		public static ForgeShop EnsureForgeShopById(string id) => Storage.EnsureByKey(id);
	}
}
