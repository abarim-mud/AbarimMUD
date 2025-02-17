using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public class Shops : MultipleFilesStorage<Shop>
	{
		public Shops() : base(s => s.Id, "shops")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var shop in this)
			{
				shop.Inventory.SetReferences();
			}
		}
	}
}
