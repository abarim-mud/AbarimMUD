using AbarimMUD.Storage;
using System;

namespace AbarimMUD.Data
{
	public class Forge : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Forge> Storage = new Forges();

		public string Id { get; set; }

		public Inventory Components { get; set; } = new Inventory();
		public int Price { get; set; }
		public Item Result { get; set; }

		public Forge CloneItem()
		{
			var result = new Forge
			{
				Id = Id,
				Components = Components.Clone(),
				Price = Price,
				Result = Result.CloneItem()
			};

			return result;
		}

		public object Clone() => CloneItem();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Forge GetForgeById(string id) => Storage.GetByKey(id);
		public static Forge EnsureForgeById(string id) => Storage.EnsureByKey(id);
	}
}
