using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.Player
{
	public class FightDetailsCommand : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: fightdetails none|damage|all");
				return false;
			}

			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Fight details could be set only for the characters.");
				return false;
			}

			FightDetails det;
			if (!Enum.TryParse(data, true, out det))
			{
				context.Send($"Unable to parse parameter '{data}'. Possible values are 'none', 'damage' and 'all'");
				return false;
			}

			character.FightDetails = det;
			character.Save();

			context.Send($"You set fight details to '{data}'.");

			return true;
		}
	}
}
