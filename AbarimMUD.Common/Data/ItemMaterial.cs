using Ur;

namespace AbarimMUD.Data
{
	public class ItemMaterial : IHasId<string>
	{
		public static readonly MultipleFilesStorage<ItemMaterial> Storage = new MultipleFilesStorage<ItemMaterial>("materials");

		public string Id { get; set; }
		public string Name { get; set; }

		public static ItemMaterial GetItemMaterialById(string id) => Storage.GetByKey(id);
		public static ItemMaterial EnsureItemMaterialById(string id) => Storage.EnsureByKey(id);
	}
}
