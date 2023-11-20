using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Say : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			sb.Append(ConsoleCommand.ForeColorMagenta);
			sb.AddTextLine(string.Format("You say '{0}'", data));
			sb.Append(ConsoleCommand.ColorClear);
			context.Send(sb.ToString());

			// Show it to others
			sb.Clear();
			sb.Append(ConsoleCommand.ForeColorMagenta);
			sb.AddTextLine(string.Format("{0} says '{1}'", context.Name, data));
			sb.Append(ConsoleCommand.ColorClear);

			foreach (var s in context.AllExceptMeInRoom())
			{
				s.Send(sb.ToString());
			}
		}
	}
}