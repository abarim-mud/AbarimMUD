using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Inventory : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			if (context.Creature.Inventory.Count > 0)
			{
				sb.AppendLine("You are carrying following items:");
			} else
			{
				sb.AppendLine("You aren't carrying any items.");
			}

			foreach (var item in context.Creature.Inventory.Items)
			{
				sb.Append(item.Info.ShortDescription);

				if (item.Quantity != 1)
				{
					sb.Append($" ({item.Quantity})");
				}

				if (context.IsStaff)
				{
					sb.Append($" (#{item.Id})");
				}

				sb.AppendLine();
			}

			sb.AppendLine();

			context.Send(sb.ToString());

			return true;
		}
	}
}
