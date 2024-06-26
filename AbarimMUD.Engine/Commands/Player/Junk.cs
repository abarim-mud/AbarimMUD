using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Junk: PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: junk <itemName>");
				return;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return;
			}

			// Remove from inv
			context.Creature.Inventory.AddItem(item.Item, -1);
			context.Send($"You junk {item.ShortDescription}.");

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();
		}
	}
}
