using AbarimMUD.Data;
using System;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Tell : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: tell <name> <message>");
				return false;
			}

			var parts = data.SplitByWhitespace(2);
			if (parts.Length != 2)
			{
				context.Send("Usage: tell <name> <message>");
				return false;
			}

			var whom = (from ch in Character.ActiveCharacters where ch.Name.StartsWith(parts[0], StringComparison.OrdinalIgnoreCase) select ch).FirstOrDefault();
			if (whom == null)
			{
				context.Send($"There's no character '{parts[0]}' online.");
				return false;
			}

			if (whom == context.Creature)
			{
				context.Send("You can't talk to yourself.");
				return false;
			}

			var ctx = (ExecutionContext)whom.Tag;

			ctx.Send($"[magenta]{context.Creature.ShortDescription} tells you '{parts[1]}'[reset]");

			return true;
		}
	}
}
