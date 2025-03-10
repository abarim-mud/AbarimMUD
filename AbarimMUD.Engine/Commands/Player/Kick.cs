using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Kick : PlayerCommand
	{
		public override bool CanFightskill => true;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ability = context.EnsureAbility("kick");
			if (ability == null)
			{
				return false;
			}

			data = data.Trim();
			if (!context.IsFighting && string.IsNullOrEmpty(data))
			{
				context.Send($"Kick who?");
				return false;
			}

			ExecutionContext target = null;
			if (!string.IsNullOrWhiteSpace(data))
			{
				target = context.Room.Find(data);
				if (target == null)
				{
					context.Send($"There isnt '{data}' in this room");
					return false;
				}

				if (target == context)
				{
					context.Send("You can't kick yourself.");
					return false;
				}

				if (target.Creature is Character)
				{
					context.Send($"You can't attack {target.ShortDescription}");
					return false;
				}
			}

			if (target == null)
			{
				target = context.FightInfo.Target;
			}

			context.Kick(target);
			Fight.Start(context, target);

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "")
		{
			return Configuration.PauseBetweenFightRoundsInMs;
		}

		public override CommandCost CalculateCost(ExecutionContext context, string data = "")
		{
			return new CommandCost(0, 0, Ability.Kick.MovesCost);
		}
	}
}
