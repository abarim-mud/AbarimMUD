using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Use: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: use <itemName>");
				return false;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return false;
			}

			if (item.Info.ItemType != ItemType.Potion)
			{
				context.Send("Only potions could be used.");
				return false;
			}

			return true;
		}
	}
}
