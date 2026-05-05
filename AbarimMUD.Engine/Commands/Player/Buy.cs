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

			InventoryRecord invItem;
			if (int.TryParse(data, out var number))
			{
				invItem = shopKeeper.Inventory.Items.ElementAtOrDefault(number - 1);
				if (invItem == null)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} I don't have item with number {number}.");
					return false;
				}
			}
			else
			{
				invItem = shopKeeper.Inventory.FindItem(data);
				if (invItem == null)
				{
					Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} I don't have '{data}'.");
					return false;
				}
			}

			var inventoryItem = invItem.Item;
			var price = context.Creature.Stats.GetBuyPrice(inventoryItem.Info.Price);
			if (context.Creature.Gold < price)
			{
				Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} You can't afford {inventoryItem.Name}.");
				return false;
			}

			context.Creature.Gold -= price;
			context.Creature.Inventory.AddItem(inventoryItem, 1);

			if (inventoryItem != null)
			{
				shopKeeper.Inventory.AddItem(inventoryItem, -1);
			}

			var character = context.Creature as Character;
			character?.Save();

			Tell.Execute(shopKeeper.GetContext(), $"{context.Creature.ShortDescription} That'll be {price}, please.");
			context.Send($"You now have {inventoryItem.Name}.");

			return true;
		}
	}
}
