using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Remove : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: remove <itemName>");
				return false;
			}

			var item = context.EnsureItemWorn(data);
			if (item == null)
			{
				return false;
			}

			if (!context.RemoveItem(item.SlotType))
			{
				return false;
			}

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();

			return true;
		}
	}
}
