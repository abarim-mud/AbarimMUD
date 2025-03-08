using AbarimMUD.Utils;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Where : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var area = context.Room.Area;

			var sb = new StringBuilder();
			sb.AppendLine($"You are in {area.Name}. Also in this area area:");

			var grid = new AsciiGrid();
			grid.SetHeader(0, "Name");
			grid.SetHeader(1, "Location");

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				var y = 0;
				foreach (var room in area.Rooms)
				{
					foreach (var ch in room.Characters)
					{
						grid.SetValue(0, y, ch.Name);
						grid.SetValue(1, y, room.ToString());
						++y;
					}
				}

				sb.Append(grid.ToString());
			}
			else
			{
				// Search in the room
				var target = context.Room.Find(data);

				if (target == null)
				{
					// Search in the area
					target = area.Find(data);
				}

				if (target == null)
				{
					sb.Append(grid.ToString());
					sb.Append($"Couldn't find anything named '{data}'.");
				} else
				{
					grid.SetValue(0, 0, target.ShortDescription);
					grid.SetValue(1, 0, target.Room.ToString());
					sb.Append(grid.ToString());
				}
			}

			context.Send(sb.ToString());

			return true;
		}
	}
}