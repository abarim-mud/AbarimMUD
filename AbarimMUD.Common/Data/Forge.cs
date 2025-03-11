using System;

namespace AbarimMUD.Data
{
	public class Forge : ICloneable
	{
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
	}
}
