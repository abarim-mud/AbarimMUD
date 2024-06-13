using AbarimMUD.Utils;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Equipment : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var grid = context.Creature.BuildEquipmentDesc();
			var sb = new StringBuilder();
			if (grid == null)
			{
				sb.AppendLine("You aren't using any items");
			}
			else
			{
				sb.AppendLine("You are using:");
				sb.Append(grid.ToString());
			}

			context.Send(sb.ToString());
		}
	}
}
