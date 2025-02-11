using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Buy : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: buy _itemName_");
			}

			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var items = Item.GetStockItems(shopKeeper.Info.Shop.Value);
			var item = (from i in items where i.MatchesKeyword(data) select i).FirstOrDefault();
			ItemInstance inventoryItem = null;
			if (item == null)
			{
				var invItem = shopKeeper.Inventory.FindItem(data);

				if (invItem == null)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} I don't have '{data}'.");
					return false;
				}

				inventoryItem = invItem.Item;
				item = inventoryItem.Info;
			}

			var price = context.Creature.Stats.GetBuyPrice(item.Price);
			if (context.Creature.Gold < price)
			{
				Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} You can't afford {item.ShortDescription}.");
				return false;
			}

			context.Creature.Gold -= price;
			context.Creature.Inventory.AddItem(new ItemInstance(item), 1);

			if (inventoryItem != null)
			{
				shopKeeper.Inventory.AddItem(inventoryItem, -1);
			}

			var character = context.Creature as Character;
			character?.Save();

			Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} That'll be {price}, please.");
			context.Send($"You now have {item.ShortDescription}.");

			return true;
		}
	}
}
