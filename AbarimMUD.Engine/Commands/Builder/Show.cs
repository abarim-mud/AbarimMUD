using AbarimMUD.Utils;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder
{
	public class Show : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 1)
			{
				context.Send($"Usage: show mobile|room [_searchPattern_]");
				return false;
			}

			var key = parts[0].ToLower();
			var storage = context.EnsureStorage(key);
			if (storage == null)
			{
				return false;
			}

			var search = string.Empty;
			if (parts.Length > 1)
			{
				search = parts[1];
			}

			var count = 0;
			var asciiGrid = new AsciiGrid();

			var area = context.CurrentArea;

			var entities = new List<object>();
			switch (key)
			{
				case "mobile":
					entities.AddRange(area.Mobiles);
					break;

				case "room":
					entities.AddRange(area.Rooms);
					break;

				default:
					context.Send($"Unknown entity type: {key}");
					return false;
			}

			foreach (var entity in entities)
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
				context.Send(asciiGrid.ToString());
				context.Send($"Total Count: {count}");
			}

			return true;
		}
	}
}