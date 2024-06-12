using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Say : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			sb.Append("[magenta]");
			sb.AppendLine(string.Format("You say '{0}'", data));
			sb.Append("[clear]");
			context.Send(sb.ToString());

			// Show it to others
			sb.Clear();
			sb.Append("[magenta]");
			sb.AppendLine(string.Format("{0} says '{1}'", context.ShortDescription, data));
			sb.Append("[clear]");

			foreach (var s in context.AllExceptMeInRoom())
			{
				s.Send(sb.ToString());
			}
		}
	}
}