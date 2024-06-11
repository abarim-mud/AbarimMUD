using AbarimMUD.Utils;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Equipment : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			AsciiGrid grid = null;

			var y = 0;
			foreach (var eq in context.Creature.Equipment.Items)
			{
				if (eq.Item == null)
				{
					continue;
				}

				if (grid == null)
				{
					grid = new AsciiGrid();
				}

				grid.SetValue(0, y, $"<worn on {eq.Slot.ToString().ToLower()}>");
				grid.SetValue(1, y, eq.Item.ShortDescription);

				++y;
			}

			var sb = new StringBuilder();
			if (grid == null)
			{
				sb.AppendLine("You aren't using any items");
			}
			else
			{
				sb.AppendLine("You are using:");
				sb.AppendLine(grid.ToString());
			}

			context.SendTextLine(sb.ToString());
		}
	}
}
