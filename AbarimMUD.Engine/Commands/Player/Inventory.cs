using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Inventory : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

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

			if (context.Creature.Inventory.Items.Length == 0)
			{
				sb.AppendLine("You aren't carrying any items.");
			}

			sb.AppendLine();

			context.Send(sb.ToString());

			return true;
		}
	}
}
