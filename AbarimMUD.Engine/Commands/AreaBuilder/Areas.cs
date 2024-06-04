using AbarimMUD.Data;
using System;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class Areas : AreaBuilderCommand
	{
		private static int AreaNumericValue(Area area)
		{
			if (string.IsNullOrEmpty(area.MinimumLevel))
			{
				return int.MinValue;
			}

			int minimumLevel;
			if (!int.TryParse(area.MinimumLevel, out minimumLevel))
			{
				if (area.MinimumLevel.Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					return int.MaxValue / 2;
				}

				return int.MaxValue;
			}

			int maximumLevel;
			if (!int.TryParse(area.MaximumLevel, out maximumLevel))
			{
				maximumLevel = 150;
			}

			var result = maximumLevel * 1000;
			result += minimumLevel;

			return result;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			var areas = (from a in Area.Storage orderby AreaNumericValue(a) select a).ToList();

			foreach (var area in areas)
			{
				sb.AppendLine(area.ToString());
			}

			sb.AppendLine();
			sb.Append($"Total areas count: {areas.Count}");

			context.SendTextLine(sb.ToString());
		}
	}
}
