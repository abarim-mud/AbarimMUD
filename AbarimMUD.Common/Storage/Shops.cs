using AbarimMUD.Data;
using System.Text.Json;
using Ur;

namespace AbarimMUD.Storage
{
	public class Shops : MultipleFilesStorage<Shop>
	{
		public Shops() : base("shops")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach (var shop in this)
			{
				shop.Inventory.SetReferences();
			}
		}
	}
}
