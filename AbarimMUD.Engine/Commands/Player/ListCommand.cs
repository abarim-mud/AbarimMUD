using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class ListCommand : PlayerCommand
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

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Item");
			grid.SetHeader(1, "Price");
			grid.SetHeader(2, "Quantity");

			var y = 0;
			for(var i = 0; i < shopKeeper.Inventory.Items.Count; ++i)
			{
				var item = shopKeeper.Inventory.Items[i].Item;
				var price = context.Creature.Stats.GetBuyPrice(item.Price);

				grid.SetValue(0, y, item.ShortDescription);
				grid.SetValue(1, y, price.ToString());
				grid.SetValue(2, y, shopKeeper.Inventory.Items[i].Quantity.ToString());

				++y;
			}

			context.Send("You can buy following items:");
			context.Send(grid.ToString());

			return true;
		}
	}
}
