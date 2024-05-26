using System.IO;
using System.Reflection;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using AbarimMUD.Data;

namespace AbarimMUD.ImportCSL
{
	internal static class Utility
	{
		private enum MobClass
		{
			Warrior,
			Thief,
			Mage,
			Cleric
		}

		private struct GuildThac0
		{
			public int Level1;
			public int Level32;

			public GuildThac0(int level1, int level32)
			{
				Level1 = level1;
				Level32 = level32;
			}
		}

		private static readonly int[][] _mobAttacksTable;
		private static readonly GuildThac0[] _guildThacs = new GuildThac0[]
		{
			new GuildThac0(20, -10),
			new GuildThac0(20, -4),
			new GuildThac0(20, 2),
			new GuildThac0(20, -2),
		};

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		static Utility()
		{
			_mobAttacksTable = new int[8][];

			_mobAttacksTable[(int)MobClass.Warrior] = new int[]
			{
				5,
				12,
				25
			};

			_mobAttacksTable[(int)MobClass.Thief] = new int[]
			{
				12,
				25
			};

			_mobAttacksTable[(int)MobClass.Mage] = new int[]
			{
				23
			};


			_mobAttacksTable[(int)MobClass.Cleric] = new int[]
			{
				20,
				30
			};
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

		private static MobClass GetMobClass(this Mobile mob)
		{
			var mc = MobClass.Warrior;
			if (mob.Flags.Contains(MobileFlags.Thief))
			{
				mc = MobClass.Thief;
			}
			else if (mob.Flags.Contains(MobileFlags.Mage))
			{
				mc = MobClass.Mage;
			}
			else if (mob.Flags.Contains(MobileFlags.Cleric))
			{
				mc = MobClass.Cleric;
			}

			return mc;
		}

		public static int GetAttacksCount(this Mobile mob)
		{
			var result = 1;

			// Area attack
			if (mob.Flags.Contains(MobileFlags.AreaAttack))
			{
				++result;
			}

			var mc = mob.GetMobClass();
			var levelsTable = _mobAttacksTable[(int)mc];

			for (var i = 0; i < levelsTable.Length; i++)
			{
				if (mob.Level < levelsTable[i])
				{
					break;
				}

				++result;
			}

			return result;
		}

		public static int GetAccuracy(this Mobile mob)
		{
			var mc = mob.GetMobClass();

			var guildThac = _guildThacs[(int)mc];
			var thac0 = guildThac.Level1 + mob.Level * (guildThac.Level32 - guildThac.Level1) / 32;

			if (thac0 < 0)
				thac0 = thac0 / 2;

			if (thac0 < -5)
				thac0 = -5 + (thac0 + 5) / 2;

			return -(thac0 - 20) * 10;
		}
	}
}
