using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Gossip : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sd = new StringBuilder();

			sd.Append(ConsoleCommand.ForeColorMagenta);
			sd.AddTextLine(string.Format("You gossip-- '{0}'", data));
			sd.Append(ConsoleCommand.ColorClear);
			context.Send(sd.ToString());

			// Show it to others
			sd.Clear();
			sd.Append(ConsoleCommand.NewLine);
			sd.Append(ConsoleCommand.ForeColorMagenta);
			sd.AddTextLine(string.Format("{0} gossips-- '{1}'", context.Name, data));
			sd.Append(ConsoleCommand.ColorClear);

			foreach (var s in context.AllExceptMe())
			{
				s.Send(sd.ToString());
			}
		}
	}
}