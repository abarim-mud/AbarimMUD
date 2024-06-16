using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Builder
{
	public class Areas : BuilderCommand
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
				if (area.MinimumLevel.EqualsToIgnoreCase("all"))
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
			var areas = (from a in Area.Storage orderby AreaNumericValue(a) select a).ToList();
			foreach (var area in areas)
			{
				context.Send(area.ToString());
			}

			context.Send();
			context.Send($"Total areas count: {areas.Count}");
		}
	}
}
