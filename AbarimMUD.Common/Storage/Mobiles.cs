using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Mobiles : MultipleFilesStorage<Mobile>
	{
		public Mobiles() : base(t => t.Id, "mobiles")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);
			result.Converters.Add(Common.ShopConverter);
			result.Converters.Add(Common.ForgeShopConverter);
			result.Converters.Add(Common.ExchangeShopConverter);
			result.Converters.Add(Common.PlayerClassConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var mobile in this)
			{
				if (mobile.Loot != null)
				{
					foreach (var loot in mobile.Loot)
					{
						loot.Items.SetReferences();
					}
				}

				if (mobile.Shop != null)
				{
					mobile.Shop = Shop.EnsureShopById(mobile.Shop.Id);
				}

				if (mobile.ForgeShop != null)
				{
					mobile.ForgeShop = ForgeShop.EnsureForgeShopById(mobile.ForgeShop.Id);
				}

				if (mobile.ExchangeShop != null)
				{
					mobile.ExchangeShop = ExchangeShop.EnsureExchangeShopById(mobile.ExchangeShop.Id);
				}

				if (mobile.Guildmaster != null)
				{
					mobile.Guildmaster = PlayerClass.EnsureClassById(mobile.Guildmaster.Id);
				}

				mobile.Experience = mobile.CalculateXpAward();
			}
		}
	}
}
