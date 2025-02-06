using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Wield : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: wear <itemName>");
				return false;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return false;
			}

			if (!context.WearItem(item.Item))
			{
				return false;
			}

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();

			return true;
		}
	}
}
