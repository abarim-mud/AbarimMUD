using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Remove : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: wear <itemName>");
				return;
			}

			var item = context.EnsureItemWorn(data);
			if (item == null)
			{
				return;
			}

			var removedItem = context.Creature.Remove(item.Slot);
			if (removedItem != null)
			{
				// Add to inv
				context.Creature.Inventory.AddItem(removedItem, 1);
				context.Send($"You stop wearing {removedItem.ShortDescription}");
			}

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();
		}
	}
}
