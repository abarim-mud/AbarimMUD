using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Kill : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else");
				return false;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Kill who?");
				return false;
			}

			var target = context.EnsureCreatureInRoom(data);
			if (target == null)
			{
				return false;
			}

			if (target == context)
			{
				context.Send("You can't attack yourself.");
				return false;
			}

			if (target.Creature is Character)
			{
				context.Send($"You can't attack {target.ShortDescription}");
				return false;
			}

			context.SingleAttack(0, target);
			Fight.Start(context, target);

			return true;
		}
	}
}
