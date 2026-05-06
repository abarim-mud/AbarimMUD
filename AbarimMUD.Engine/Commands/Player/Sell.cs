using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Sell : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: sell _itemName_");
			}

			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var shopKeeperContext = shopKeeper.GetContext();

			var invItem = context.Creature.Inventory.FindItem(data);
			if (invItem == null)
			{
				context.Send($"You don't seem to have '{data}'");
				return false;
			}

			var item = invItem.Item;
			if (!shopKeeper.Shop.ItemTypes.Contains(item.ItemType))
			{
				shopKeeperContext.Tell(context, $"I don't deal in those.");
				return false;
			}

			var price = Configuration.SellPricePercentage * item.Price / 100;

			context.Creature.Gold += price;
			context.Creature.Inventory.AddItem(item, -1);

			shopKeeper.ShopAddItem(item, 1);

			var character = context.Creature as Character;
			character?.Save();

			shopKeeperContext.Tell(context, $"You'll get {price} coins for it!");
			context.Send($"{shopKeeper.ShortDescription} now has {invItem.Name}.");

			return true;
		}
	}
}
