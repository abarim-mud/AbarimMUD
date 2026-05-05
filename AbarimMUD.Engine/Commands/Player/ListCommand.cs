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

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Number");
			grid.SetHeader(1, "Item");
			grid.SetHeader(2, "Price");
			grid.SetHeader(3, "Quantity");

			for (var i = 0; i < shopKeeper.Inventory.Items.Count; ++i)
			{
				var item = shopKeeper.Inventory.Items[i].Item;
				var price = context.Creature.Stats.GetBuyPrice(item.Price);

				grid.SetValue(0, i, (i + 1).ToString());
				grid.SetValue(1, i, item.Name);
				grid.SetValue(2, i, price.ToString());
				grid.SetValue(3, i, shopKeeper.Inventory.Items[i].Quantity.ToString());
			}

			context.Send("You can buy following items:");
			context.Send(grid.ToString());

			return true;
		}
	}
}
