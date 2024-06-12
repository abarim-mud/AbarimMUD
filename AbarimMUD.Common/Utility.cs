using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD
{
	public static class Utility
	{
		public static Random Random { get; } = new Random();

		public static bool RollPercentage(int percentage)
		{
			if (percentage < 1)
			{
				// Fail
				return false;
			}

			if (percentage >= 100)
			{
				// Win
				return true;
			}

			var rnd = Random.Next(1, 101);
			return rnd <= percentage;
		}

		public static int RandomRange(int min, int max)
		{
			if (min >= max)
			{
				return min;
			}

			return Random.Next(min, max + 1);
		}

		public static JsonSerializerOptions CreateDefaultOptions()
		{
			var result = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
				IncludeFields = true,
			};

			result.Converters.Add(new JsonStringEnumConverter());

			return result;
		}

		public static float Clamp(float val, float min = 0.0f, float max = 1.0f)
		{
			if (val < min)
			{
				return min;
			}

			if (val > max)
			{
				return max;
			}

			return val;
		}

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

		public static bool StartsWithPattern(this string[] keywords, string[] pattern)
		{
			foreach (var s in pattern)
			{
				var found = (from k in keywords where k.StartsWith(s, StringComparison.OrdinalIgnoreCase) select k).Any();

				if (!found)
				{
					return false;
				}
			}

			return true;
		}

		public static IReadOnlyDictionary<string, string> BuildInfoDict<T>(this T obj)
		{
			var type = typeof(T);

			var values = new Dictionary<string, string>();

			var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);
			foreach (var prop in props)
			{
				var value = prop.GetValue(obj, null);
				values[prop.Name] = value != null ? value.ToString() : string.Empty;
			}

			return values;
		}
	}
}
