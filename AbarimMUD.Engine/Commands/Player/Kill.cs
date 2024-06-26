using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Kill : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else");
				return;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Kill who?");
				return;
			}

			var target = context.Room.Find(data);
			if (target == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return;
			}

			if (target == context)
			{
				context.Send("You can't attack yourself.");
				return;
			}

			if (target.Creature is Character)
			{
				context.Send($"You can't attack {target.ShortDescription}");
				return;
			}

			context.SingleAttack(0, target);
			Fight.Start(context, target);
		}
	}
}
