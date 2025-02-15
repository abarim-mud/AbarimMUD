using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class ExchangeCommand : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.ExchangeShop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var exchanges = shopKeeper.Info.ExchangeShop.Exchanges;
			if (string.IsNullOrEmpty(data))
			{
				var grid = new AsciiGrid();
				grid.SetHeader(0, "#");
				grid.SetHeader(1, "Item");
				grid.SetHeader(2, "Price");

				var y = 0;
				foreach (var f in exchanges)
				{
					grid.SetValue(0, y, (y + 1).ToString());
					grid.SetValue(1, y, f.Item.ShortDescription);

					var sb = new StringBuilder();

					for (var i = 0; i < f.Price.Count; i++)
					{
						var cp = f.Price[i];
						sb.Append(cp.ShortDescription);

						if (cp.Quantity > 1)
						{
							sb.Append($" ({cp.Quantity})");
						}

						if (i < f.Price.Count - 1)
						{
							sb.Append(", ");
						}
					}

					grid.SetValue(2, y, sb.ToString());

					++y;
				}

				context.Send("You can exchange following items:");
				context.Send(grid.ToString());

				return true;
			}

			int id;
			if (!context.EnsureInt(data, out id))
			{
				return false;
			}

			var index = id - 1;
			if (id < 1 || index >= exchanges.Count)
			{
				context.Send($"Unknown exchange #{id}.");
				return false;
			}

			var exchange = exchanges[index];

			// Check cps
			var creature = context.Creature;
			var inv = creature.Inventory;
			foreach (var cp in exchange.Price)
			{
				var invItem = (from i in inv where ItemInstance.AreEqual(i.Item, cp.Item) select i).FirstOrDefault();
				if (invItem == null)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} You don't have any {cp.Item.ShortDescription}.");
					return false;
				}

				if (invItem.Quantity < cp.Quantity)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} You don't have enough amount of {cp.Item.ShortDescription}.");
					return false;
				}
			}

			// Create item
			var item = exchange.Item.Clone();
			inv.AddItem(item, 1);

			foreach (var cp in exchange.Price)
			{
				inv.AddItem(cp.Item, -cp.Quantity);
			}

			context.Send($"{shopKeeper} gives you {exchange.Item.ShortDescription} in exchange for some items.");
			Tell.Execute(shopKeeper.GetContext(), $"{creature.ShortDescription} There you go, {creature.ShortDescription}.");

			var character = creature as Character;
			character?.Save();

			return true;
		}
	}
}
