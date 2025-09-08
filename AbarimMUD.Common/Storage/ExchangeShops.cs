using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class ExchangeShops : MultipleFilesStorage<ExchangeShop>
	{
		public ExchangeShops() : base(f => f.Id, "exchangeShops")
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
				foreach (var exchange in exchangeShop.Exchanges)
				{
					exchange.Item.SetReferences();
					exchange.Price.SetReferences();
				}
			}
		}
	}
}
