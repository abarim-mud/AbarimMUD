using System;

namespace AbarimMUD.Utils
{
	public static class StringUtils
	{
		public static string CasedName(this string name)
		{
			if (name == null)
			{
				return null;
			}

			name = name.Trim();
			if (string.IsNullOrEmpty(name))
			{
				return string.Empty;
			}

			if (name.Length == 1)
			{
				return name.ToUpper();
			}

			return char.ToUpper(name[0]) + name.Substring(1, name.Length - 1).ToLower();
		}

		public static bool TryParseEnumUncased<T>(this string name, out T value) where T : struct
		{
			return Enum.TryParse(name.CasedName(), out value);
		}

		public static string[] SplitByWhitespace(this string value, int? maxCount = null)
		{
			if (maxCount == null)
			{
				return value.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
			}

			return value.Split(new char[0], maxCount.Value, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}