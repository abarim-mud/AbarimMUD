using System;
using System.Xml.Linq;
using System.Collections.Generic;
using AbarimMUD.Import;

namespace AbarimMUD.ImportCSL
{
	internal static class Utility
	{
		public static string GetString(this XElement element, string name, string def = null)
		{
			var el = element.Element(name);
			if (el != null)
			{
				return el.Value;
			}

			var attr = element.Attribute(name);
			if (attr != null)
			{
				return attr.Value;
			}
			return def;
		}

		public static int GetInt(this XElement element, string name, int def = 0)
		{
			var val = element.GetString(name);
			if (string.IsNullOrEmpty(val))
			{
				return def;
			}

			return int.Parse(val);
		}

		public static int EnsureInt(this XElement element, string name)
		{
			var val = element.GetString(name);
			if (string.IsNullOrEmpty(val))
			{
				throw new Exception($"Integer field {name} either doesn't exist or empty.");
			}

			return int.Parse(val);
		}

		private static string UpdateEnumValue(this string val)
		{
			return val.Trim().Replace(" ", "").Replace("-", "");
		}

		public static T GetEnum<T>(this XElement element, string name, T def = default(T)) where T : struct
		{
			var val = element.GetString(name);
			if (string.IsNullOrEmpty(val))
			{
				return def;
			}

			val = val.UpdateEnumValue();
			return Enum.Parse<T>(val, true);
		}

		public static T EnsureEnum<T>(this XElement element, string name) where T : struct
		{
			var val = element.GetString(name);
			if (string.IsNullOrEmpty(val))
			{
				throw new Exception($"Enum field {name} of type {typeof(T)} either doesn't exist or empty.");
			}

			val = val.UpdateEnumValue();
			return Enum.Parse<T>(val, true);
		}

		public static HashSet<T> ParseFlags<T>(this string value) where T : struct
		{
			var result = new HashSet<T>();
			var parts = value.Split(' ');
			foreach (var part in parts)
			{
				if (string.IsNullOrEmpty(part))
				{
					continue;
				}

				var val = part.UpdateEnumValue();
				var r = Enum.Parse<T>(val, true);
				result.Add(r);
			}

			return result;
		}

		public static HashSet<T> ParseFlags<T>(this XElement element, string name) where T : struct
		{
			var val = element.GetString(name);
			if (string.IsNullOrEmpty(val))
			{
				return new HashSet<T>();
			}

			return ParseFlags<T>(val);
		}

		public static Dice EnsureDice(this XElement element, string name)
		{
			var sides = element.EnsureInt(name + "Sides");
			var count = element.EnsureInt(name + "Count");
			var bonus = element.EnsureInt(name + "Bonus");

			return new Dice(sides, count, bonus);
		}

		public static Dice GetDice(this XElement element, string name, Dice def)
		{
			if (element.Element(name + "Sides") == null ||
				element.Element(name + "Count") == null ||
				element.Element(name + "Bonus") == null)
			{
				return def;
			}

			var sides = element.EnsureInt(name + "Sides");
			var count = element.EnsureInt(name + "Count");
			var bonus = element.EnsureInt(name + "Bonus");

			return new Dice(sides, count, bonus);
		}
	}
}
