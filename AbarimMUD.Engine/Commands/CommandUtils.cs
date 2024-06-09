using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{
		public static void ParseCommand(this string cmd, out string cmdText, out string cmdData)
		{
			cmdText = string.Empty;
			cmdData = string.Empty;
			var i = 0;
			for (; i < cmd.Length; ++i)
			{
				if (cmd[i] == ' ')
				{
					break;
				}

				cmdText += cmd[i];
			}

			cmdText = cmdText.Trim().ToLower();

			if (i < cmd.Length)
			{
				cmdData = cmd.Substring(i + 1);
			}
		}

		public static bool Parse(this string cmd, out int p1, out string p2, out int p3, out int p4, out int p5)
		{
			p2 = string.Empty;
			p1 = p3 = p4 = p5 = 0;
			var parts = cmd.Split(' ');
			if (parts.Length != 5)
			{
				return false;
			}

			if (!int.TryParse(parts[0], out p1))
			{
				return false;
			}

			p2 = parts[1];

			if (!int.TryParse(parts[2], out p3))
			{
				return false;
			}

			if (!int.TryParse(parts[3], out p4))
			{
				return false;
			}

			if (!int.TryParse(parts[4], out p5))
			{
				return false;
			}

			return true;
		}

		public static bool Parse(this string cmd, out int p1, out int p2)
		{
			p1 = p2 = 0;
			var parts = cmd.Split(' ');
			if (parts.Length != 2)
			{
				return false;
			}

			if (!int.TryParse(parts[0], out p1))
			{
				return false;
			}

			if (!int.TryParse(parts[1], out p2))
			{
				return false;
			}

			return true;
		}

		public static Race EnsureRace(this ExecutionContext context, string name)
		{
			var race = Race.GetRaceByName(name);
			if (race == null)
			{
				context.SendTextLine($"Unable to find race '{name}'");
			}

			return race;
		}

		public static GameClass EnsureClass(this ExecutionContext context, string name)
		{
			var cls = GameClass.GetClassByName(name);
			if (cls == null)
			{
				context.SendTextLine($"Unable to find class '{name}'");
			}

			return cls;
		}
	}
}