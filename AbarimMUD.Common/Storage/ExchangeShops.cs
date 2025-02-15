using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class ExchangeShops : MultipleFilesStorage<ExchangeShop>
	{
		public ExchangeShops() : base(f => f.Id, "ExchangeShops")
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

			foreach (var exchangeShop in this)
			{
				foreach(var exchange in exchangeShop.Exchanges)
				{
					exchange.Item.Info = Item.EnsureItemById(exchange.Item.Id);

					foreach(var invItem in exchange.Price)
					{
						invItem.Info = Item.EnsureItemById(invItem.Id);
					}

				}
			}
		}
	}
}
