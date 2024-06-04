using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Where : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var area = context.CurrentRoom.Area;

			var sb = new StringBuilder();
			sb.Append(ConsoleCommand.ForeColorCyan);

			var name = area.Name;
			if (context.IsStaff)
			{
				name += string.Format(" (#{0})", area.Name);
			}

			sb.AddTextLine(name);
			sb.Append(ConsoleCommand.ColorClear);

			context.Send(sb.ToString());
		}
	}
}