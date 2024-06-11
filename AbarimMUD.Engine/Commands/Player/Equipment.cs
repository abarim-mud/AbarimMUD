using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Equipment : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			var found = false;
			foreach(var eq in context.Creature.Equipment.Items)
			{
				if (eq.Item == null)
				{
					continue;
				}

				if (!found)
				{
					sb.AppendLine("You are using:");
					found = true;
				}

				sb.AppendLine($"<worn on {eq.Slot.ToString().ToLower()}>\t\t{eq.Item.ShortDescription}");
			}

			if (!found)
			{
				sb.AppendLine("You aren't using any items");
			}

			context.SendTextLine(sb.ToString());
		}
	}
}
