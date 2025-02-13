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

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var shop in this)
			{
				foreach (var invItem in shop.Inventory.Items)
				{
					invItem.Item.Info = Item.EnsureItemById(invItem.Item.Info.Id);
				}
			}
		}
	}
}
