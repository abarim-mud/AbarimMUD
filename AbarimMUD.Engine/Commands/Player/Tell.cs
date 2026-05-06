using AbarimMUD.Data;
using AbarimMUD.Utils;
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

			var whom = (from ch in Character.ActiveCharacters where ch.Name.EqualsToIgnoreCase(parts[0]) select ch).FirstOrDefault();
			if (whom == null)
			{
				context.Send($"There's no character '{parts[0]}' online.");
				return false;
			}

			return context.Tell(whom.GetContext(), parts[1]);
		}
	}
}
