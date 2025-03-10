using AbarimMUD.Data;
using System.Text;
using System.Linq;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Player
{
	public sealed class Help : PlayerCommand
	{
		private const int CommandsPerRow = 4;

		private static void AppendCommands(StringBuilder sb, Role role, ref int count)
		{
			sb.AppendLine($"[white]{role} commands:[reset]");

			var commands = (from cmd in AllCommands where cmd.Value.RequiredRole == role select cmd.Value).ToArray();

			var grid = new AsciiGrid();

			for(var i = 0; i < commands.Length; ++i)
			{
				var cmd = commands[i];

				grid.SetValue(i % CommandsPerRow, i / CommandsPerRow, cmd.Name.ToLower());
			}

			sb.AppendLine(grid.ToString());

			count += commands.Length;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();
			var count = 0;

			for(var i = (int)Role.Owner; i >= (int)Role.Player; --i)
			{
				if ((int)context.Role < i)
				{
					continue;
				}

				AppendCommands(sb, (Role)i, ref count);
			}

			sb.AppendLine($"Total commands: {count}");

			context.Send(sb.ToString());

			return true;
		}
	}
}
