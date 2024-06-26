using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Where : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var area = context.Room.Area;

			var sb = new StringBuilder();
			sb.Append("[cyan]");

			var name = area.Name;
			if (context.IsStaff)
			{
				name += string.Format(" (#{0})", area.Name);
			}

			sb.AppendLine(name);
			sb.Append("[reset]");

			context.Send(sb.ToString());
		}
	}
}