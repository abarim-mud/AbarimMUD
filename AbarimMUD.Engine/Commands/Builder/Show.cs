using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Utils;
using System.Text;

namespace AbarimMUD.Commands.Builder
{
	public class Show : BuilderCommand
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

			var query = storage.Lookup(context, search);
			foreach (var entity in query)
			{
				asciiGrid.SetValue(0, count, entity.ToString());

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