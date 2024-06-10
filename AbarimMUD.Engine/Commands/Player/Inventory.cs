using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Inventory : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sd = new StringBuilder();

			foreach (var item in context.Creature.Inventory.Items)
			{
				sd.Append(item.Info.ShortDescription);

				if (item.Quantity != 1)
				{
					sd.Append($" ({item.Quantity})");
				}

				if (context.IsStaff)
				{
					sd.Append($" (#{item.Id})");
				}

				sd.AppendLine();
			}

			sd.AppendLine();

			context.Send(sd.ToString());
		}
	}
}
