using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class Slain : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Usage: slain _target_");
				return;
			}

			var lookContext = context.CurrentRoom.Find(data);
			if (lookContext == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return;
			}

			lookContext.Creature.Slain();

			var ripMessage = $"{lookContext.Creature.ShortDescription} is dead! R.I.P.";
			context.Send(ripMessage);

			foreach (var roomContext in context.AllExceptMeInRoom())
			{
				roomContext.Send(ripMessage);
			}

			var character = context.Creature as Character;
			var targetMobile = lookContext.Creature as MobileInstance;
			if (character != null && targetMobile != null)
			{
				var xpAward = targetMobile.Stats.XpAward;

				var lastLevel = character.Level;
				character.Experience += xpAward;
				context.Send($"Total exp for kill is {xpAward.FormatBigNumber()}.");

				// Append level up messages
				for (var level = lastLevel + 1; level <= character.Level; ++level)
				{
					var previousHp = character.Class.HitpointsRange.CalculateValue(level - 1);
					var newHp = character.Class.HitpointsRange.CalculateValue(level);
					context.Send($"Welcome to the level {level}! You gained {newHp - previousHp} hitpoints.");
				}

				character.Wealth += targetMobile.Info.Wealth;
				context.Send($"You get {targetMobile.Info.Wealth.FormatBigNumber()} from the corpse of {targetMobile.ShortDescription}.");

				var roomMessage = $"{context.ShortDescription} gets gold coins from the corpse of {targetMobile.ShortDescription}.";
				foreach (var roomContext in context.AllExceptMeInRoom())
				{
					roomContext.Send(roomMessage);
				}
			}
		}
	}
}
