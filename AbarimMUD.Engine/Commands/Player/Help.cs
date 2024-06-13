using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Help : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			var count = 0;
			foreach (var ac in AllCommands)
			{
				if (ac.Value.RequiredType <= context.Role)
				{
					sb.AppendLine(ac.Key);
					++count;
				}
			}

			sb.AppendLine($"Total commands: {count}");

			context.Send(sb.ToString());
		}
	}
}
