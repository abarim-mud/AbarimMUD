using AbarimMUD.Data;
using System.Collections.Generic;

namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{

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

		public static bool TryParseArgument(this string[] args, int index, out int value)
		{
			value = 0;
			if (index >= args.Length)
			{
				return false;
			}

			return int.TryParse(args[index], out value);
		}
	}
}