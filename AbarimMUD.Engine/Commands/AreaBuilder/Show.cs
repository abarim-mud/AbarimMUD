using AbarimMUD.Commands.AreaBuilder.OLCUtils;
using AbarimMUD.Utils;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class Show : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 1)
			{
				context.Send($"Usage: show {OLCManager.KeysString} [_searchPattern_]");
			}

			var key = parts[0].ToLower();
			var storage = context.EnsureStorage(key);
			if (storage == null)
			{
				return;
			}

			var search = string.Empty;
			if (parts.Length > 1)
			{
				search = parts[1];
			}

			var count = 0;
			var asciiGrid = new AsciiGrid();
			foreach (var entity in storage)
			{
				if (!string.IsNullOrEmpty(search) && !entity.Id.Contains(search, System.StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				asciiGrid.SetValue(0, count, entity.Id);

				count++;
			}

			if (count == 0)
			{
				context.Send($"There's no entities of type {key}.");
			}
			else
			{
				var sb = new StringBuilder();

				sb.Append(asciiGrid.ToString());
				sb.AppendLine($"Total Count: {count}");

				context.Send(sb.ToString());
			}
		}
	}
}
