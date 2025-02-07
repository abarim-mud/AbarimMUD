using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class ListCommand: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var items = Item.GetStockItems(shopKeeper.Info.Shop.Value);

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Item");
			grid.SetHeader(1, "Price");
			grid.SetHeader(2, "Quantity");

			for (var i = 0; i < items.Length; ++i)
			{
				grid.SetValue(0, i, items[i].ShortDescription);
				grid.SetValue(1, i, items[i].Price.ToString());
				grid.SetValue(2, i, "infinite");
			}

			context.Send("You can buy following items:");
			context.Send(grid.ToString());

			return true;
		}
	}
}
