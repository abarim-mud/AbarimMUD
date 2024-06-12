using System.Collections.Generic;

namespace AbarimMUD.Utils
{
	internal static class StringUtils
	{
		public static string ToAsciiGridString(this IReadOnlyDictionary<string, string> dict)
		{
			var grid = new AsciiGrid();

			var y = 0;
			foreach(var pair in dict)
			{
				grid.SetValue(0, y, pair.Key);
				grid.SetValue(1, y, pair.Value);

				++y;
			}

			return grid.ToString();
		}

		public static string JoinKeywords(this HashSet<string> keywords) => string.Join(" ", keywords);
	}
}
