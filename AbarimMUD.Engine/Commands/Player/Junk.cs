using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Junk: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: junk <itemName>");
				return false;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return false;
			}

			// Remove from inv
			context.Creature.Inventory.AddItem(item.Item, -1);
			context.Send($"You junk {item.Name}.");

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();

			return true;
		}
	}
}
