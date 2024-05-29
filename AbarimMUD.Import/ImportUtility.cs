using System.Reflection;
using AbarimMUD.Data;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace AbarimMUD.Import
{
	public static class ImportUtility
	{
		private static readonly Regex CreditsRegEx1 = new Regex(@"^[\{\[]?\s*(\w+)\s*[\}\]]\s*(\w+)");
		private static readonly Regex CreditsRegEx2 = new Regex(@"^[\{\[]\s*(\w+)\s*-?\s*(\w+)\s*[\}\]]\s*(\w+)");

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

		static ImportUtility()
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

		public static void Log(string message) => Console.WriteLine(message);

		public static bool ParseLevelsBuilds(this Area area)
		{
			// Try to get levels range from the credits
			var c = area.Credits.Trim();
			var match = CreditsRegEx1.Match(c);
			if (match.Success)
			{
				area.MinimumLevel = area.MaximumLevel = match.Groups[1].Value;
				if (string.IsNullOrEmpty(area.Builders))
				{
					area.Builders = match.Groups[2].Value;
				}

				Log($"Regex1 worked: parsed {area.MinimumLevel}/{area.Builders} from {c}");
			}
			else
			{

				match = CreditsRegEx2.Match(c);
				if (match.Success)
				{
					area.MinimumLevel = match.Groups[1].Value;
					area.MaximumLevel = match.Groups[2].Value;

					if (string.IsNullOrEmpty(area.Builders))
					{
						area.Builders = match.Groups[3].Value;
					}

					Log($"Regex2 worked: parsed [{area.MinimumLevel} {area.MaximumLevel}]/{area.Builders} from {c}");
				}
			}

			if (!match.Success)
			{
				Log($"Couldn't parse levels/builders info from {c}");
			}

			return match.Success;
		}

		private static MobClass GetMobClass(this Mobile mob, string guild)
		{
			if (guild.Equals("warrior", StringComparison.OrdinalIgnoreCase))
			{
				return MobClass.Warrior;
			} else if (guild.Equals("mage", StringComparison.OrdinalIgnoreCase))
			{
				return MobClass.Mage;
			}
			else if (guild.Equals("healer", StringComparison.OrdinalIgnoreCase))
			{
				return MobClass.Cleric;
			}
			else if (guild.Equals("thief", StringComparison.OrdinalIgnoreCase))
			{
				return MobClass.Thief;
			}

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

		public static int GetAttacksCount(this Mobile mob, string guild)
		{
			var result = 1;

			// Area attack
			if (mob.Flags.Contains(MobileFlags.AreaAttack))
			{
				++result;
			}

			var mc = mob.GetMobClass(guild);
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

		public static int GetAccuracy(this Mobile mob, string guild, int hitRoll)
		{
			var mc = mob.GetMobClass(guild);

			var guildThac = _guildThacs[(int)mc];
			var thac0 = guildThac.Level1 + mob.Level * (guildThac.Level32 - guildThac.Level1) / 32;
			thac0 -= hitRoll;

			if (thac0 < 0)
				thac0 = thac0 / 2;

			if (thac0 < -5)
				thac0 = -5 + (thac0 + 5) / 2;

			return -(thac0 - 20) * 10;
		}
	}
}
