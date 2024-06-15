using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public static bool StartsWithPattern(this HashSet<string> keywords, string pattern)
		{
			return (from k in keywords where k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) select k).Any();
		}

		public static bool EqualsToIgnoreCase(this string name, string otherName) => string.Equals(name, otherName, StringComparison.OrdinalIgnoreCase);

		public static T FindAttribute<T>(this MemberInfo property) where T : Attribute
		{
			var result = (from T a in property.GetCustomAttributes<T>(true) select a).FirstOrDefault();

			return result;
		}

		public static bool HasAttribute<T>(this MemberInfo property) where T : Attribute
		{
			return property.FindAttribute<T>() != null;
		}

		public static T FindAttribute<T>(this Type type) where T : Attribute
		{
			var result = (from T a in type.GetCustomAttributes<T>(true) select a).FirstOrDefault();

			return result;
		}

		public static bool HasAttribute<T>(this Type type) where T : Attribute
		{
			return type.FindAttribute<T>() != null;
		}

		public static MethodInfo EnsureMethod(this Type type, string methodName)
		{
			var result = type.GetMethod(methodName);
			if (result == null)
			{
				throw new Exception($"Unable to find method {methodName}");
			}

			return result;
		}

		public static int CalculateValue(this ValueRange? range, int level,
			ValueRangeGrowthType growthType = ValueRangeGrowthType.Linear,
			int defaultValue = 0)
		{
			if (range == null)
			{
				return defaultValue;
			}

			return range.Value.CalculateValue(level, growthType);
		}

		public static bool IsNullable(this Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
		}

		public static bool IsNullablePrimitive(this Type type)
		{
			return type.IsNullable() && type.GenericTypeArguments[0].IsPrimitive;
		}

		public static bool IsNullableEnum(this Type type)
		{
			return type.IsNullable() && type.GenericTypeArguments[0].IsEnum;
		}

		public static Type GetNullableType(this Type type)
		{
			return type.GenericTypeArguments[0];
		}
	}
}
