using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Who : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();
			var staff = (from c in Character.ActiveCharacters where c.Role != Role.Player orderby c.Role descending select c).ToList();

			AsciiGrid grid;
			if (staff.Count > 0)
			{
				grid = new AsciiGrid();
				grid.SetHeader(0, "Staff");

				for(var y = 0; y < staff.Count; ++y)
				{
					var c = staff[y];

					grid.SetValue(0, y, $"[**{c.Role}**]");
					grid.SetValue(1, y, $"{c.NameAndTitleAndOffline()}");
				}

				sb.AppendLine(grid.ToString());
			}

			var players = (from c in Character.ActiveCharacters where c.Role == Role.Player orderby c.Level descending select c).ToList();

			grid = new AsciiGrid();
			grid.SetHeader(0, "Players");

			for(var y = 0; y < players.Count; ++y)
			{
				var c = players[y];

				grid.SetValue(0, y, $"[{c.ClassName} {c.Level}]");
				grid.SetValue(1, y, $"{c.NameAndTitleAndOffline()}");
			}

			sb.AppendLine(grid.ToString());

			context.Send(sb.ToString());

			return true;
		}
	}
}
