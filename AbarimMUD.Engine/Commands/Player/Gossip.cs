using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Gossip : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			sb.Append("[magenta]");
			sb.AppendLine(string.Format("You gossip-- '{0}'", data));
			sb.Append("[reset]");
			context.Send(sb.ToString());

			// Show it to others
			sb.Clear();
			sb.AppendLine();
			sb.Append("[magenta]");
			sb.AppendLine(string.Format("{0} gossips-- '{1}'", context.ShortDescription, data));
			sb.Append("[reset]");

			context.SendAllExceptMe(sb.ToString());

			return true;
		}
	}
}