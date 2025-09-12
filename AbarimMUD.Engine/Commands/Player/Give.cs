using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Give : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: give <itemName> <player>");
				return false;
			}

			var parts = data.SplitByWhitespace(2);
			if (parts.Length != 2)
			{
				context.Send("Usage: give <itemName> <player>");
				return false;
			}

			// Find shopkeeper
			var player = (from cr in context.Room.Characters where cr.Name.EqualsToIgnoreCase(parts[1]) select cr).FirstOrDefault();
			if (player == null)
			{
				context.Send($"There isn't {parts[1]} in this room!");
				return false;
			}

			var invItem = context.Creature.Inventory.FindItem(parts[0]);
			if (invItem == null)
			{
				context.Send($"You don't seem to have '{parts[0]}'");
				return false;
			}

			var item = invItem.Item;
			
			context.Creature.Inventory.AddItem(item, -1);
			player.Inventory.AddItem(item, 1);

			var character = context.Creature as Character;
			character?.Save();

			player.Save();

			player.GetContext().Send($"{context.ShortDescription} gave you {item.Name}.");
			context.Send($"You gave {item.Name} to {player.ShortDescription}.");

			return true;
		}
	}
}
