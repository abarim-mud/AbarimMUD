using System;
using System.Collections.Generic;
using System.Text;

namespace AbarimMUD.Utils
{
	internal static class StringUtils
	{
		public static string ToAsciiGridString(this IReadOnlyDictionary<string, string> dict)
		{
			var grid = new AsciiGrid();

			var y = 0;
			foreach (var pair in dict)
			{
				grid.SetValue(0, y, pair.Key);
				grid.SetValue(1, y, pair.Value);

				++y;
			}

			return grid.ToString();
		}

		public static string JoinKeywords(this HashSet<string> keywords) => string.Join(" ", keywords);

		public static string FormatMessage(this string message, Dictionary<string, string> variables)
		{
			if (string.IsNullOrEmpty(message))
			{
				return message;
			}

			var result = new StringBuilder();
			var variable = new StringBuilder();

			var readingVariable = false;
			for (var i = 0; i < message.Length; ++i)
			{
				var c = message[i];

				if (c == '{' && !readingVariable)
				{
					variable.Clear();
					readingVariable = true;
				}
				else if (c == '}' && readingVariable)
				{
					var v = variable.ToString().ToLower();

					if (!variables.TryGetValue(v, out var value))
					{
						throw new Exception($"Unknown variable '{v}'");
					}

					result.Append(value);
					readingVariable = false;
				}
				else
				{
					if (!readingVariable)
					{
						result.Append(c);
					}
					else
					{
						variable.Append(c);
					}
				}
			}

			if (readingVariable)
			{
				throw new Exception("Unfinished variable name");
			}

			return result.ToString();
		}
	}
}
