using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class ListCommand : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var shop = shopKeeper.Shop;

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Number");
			grid.SetHeader(1, "Item");
			grid.SetHeader(2, "Price");
			grid.SetHeader(3, "Quantity");

			// Unlimited items first
			var y = 0;
			foreach (var item in shop.UnlimitedItems)
			{
				grid.SetValue(0, y, (y + 1).ToString());
				grid.SetValue(1, y, item.Name);
				grid.SetValue(2, y, item.Price.FormatBigNumber());
				grid.SetValue(3, y, "Unlimited");
				++y;
			}

			// Then limited items
			for (var i = 0; i < shopKeeper.ShopInventory.Count; ++i)
			{
				var rec = shopKeeper.ShopInventory[i];
				var item = rec.Item;

				grid.SetValue(0, y, (y + 1).ToString());
				grid.SetValue(1, y, item.Name);
				grid.SetValue(2, y, item.Price.FormatBigNumber());
				grid.SetValue(3, y, rec.Quantity.ToString());

				++y;
			}

			context.Send("You can buy following items:");
			context.Send(grid.ToString());

			return true;
		}
	}
}
