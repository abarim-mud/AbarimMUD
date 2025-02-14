using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Sell: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: sell _itemName_");
			}

			// Find shopkeeper
			var shopKeeper = (from cr in context.Room.Mobiles where cr.Info.Shop != null select cr).FirstOrDefault();
			if (shopKeeper == null)
			{
				context.Send("Sorry, but you cannot do that here!");
				return false;
			}

			var invItem = context.Creature.Inventory.FindItem(data);
			if (invItem == null)
			{
				context.Send($"You don't seem to have '{data}'");
				return false;
			}

			var item = invItem.Item;
			if (!shopKeeper.Info.Shop.ItemTypes.Contains(item.ItemType))
			{
				Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} I don't deal in those.");
				return false;
			}

			var price = context.Creature.Stats.GetSellPrice(item.Price);

			context.Creature.Gold += price;
			context.Creature.Inventory.AddItem(item, -1);

			shopKeeper.Inventory.AddItem(item, 1);

			var character = context.Creature as Character;
			character?.Save();

			Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} You'll get {price} coins for it!");
			context.Send($"{shopKeeper.ShortDescription} now has {invItem.ShortDescription}.");

			return true;
		}
	}
}
