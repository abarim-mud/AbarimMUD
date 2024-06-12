using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Gossip : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sd = new StringBuilder();

			sd.Append("[magenta]");
			sd.AppendLine(string.Format("You gossip-- '{0}'", data));
			sd.Append("[clear]");
			context.Send(sd.ToString());

			// Show it to others
			sd.Clear();
			sd.AppendLine();
			sd.Append("[magenta]");
			sd.AppendLine(string.Format("{0} gossips-- '{1}'", context.ShortDescription, data));
			sd.Append("[clear]");

			foreach (var s in context.AllExceptMe())
			{
				s.Send(sd.ToString());
			}
		}
	}
}