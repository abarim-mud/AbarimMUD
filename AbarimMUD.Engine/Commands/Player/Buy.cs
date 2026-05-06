using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Buy : PlayerCommand
	{
		public override string HelpText => "Usage: buy _name_|_number_";

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				ShowHelp(context);
				return true;
			}

			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var shopKeeperContext = shopKeeper.GetContext();

			ItemInstance inventoryItem;
			if (int.TryParse(data, out var number))
			{
				inventoryItem = shopKeeper.GetShopItemByNumber(number);
				if (inventoryItem == null)
				{
					shopKeeperContext.Tell(context, $"I don't have item with number {number}.");
					return false;
				}
			}
			else
			{
				inventoryItem = shopKeeper.GetShopItemByKeyword(data);
				if (inventoryItem == null)
				{
					shopKeeperContext.Tell(context, $"I don't have '{data}'.");
					return false;
				}
			}

			var price = inventoryItem.Info.Price;
			if (context.Creature.Gold < price)
			{
				shopKeeperContext.Tell(context, $"You can't afford {inventoryItem.Name}.");
				return false;
			}

			context.Creature.Gold -= price;
			context.Creature.Inventory.AddItem(inventoryItem, 1);

			shopKeeper.ShopAddItem(inventoryItem, -1);

			var character = context.Creature as Character;
			character?.Save();

			shopKeeperContext.Tell(context, $"That'll be {price}, please.");
			context.Send($"You now have {inventoryItem.Name}.");

			return true;
		}
	}
}
