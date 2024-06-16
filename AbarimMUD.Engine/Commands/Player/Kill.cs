namespace AbarimMUD.Commands.Player
{
	public class Kill : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (context.Creature.FightsWith != null)
			{
				context.Send($"You're too busy fighting with someone else");
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Kill who?");
				return;
			}

			var lookContext = context.CurrentRoom.Find(data);
			if (lookContext == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return;
			}

			var asMobileContext = lookContext as MobileExecutionContext;
			if (asMobileContext == null)
			{
				context.Send($"You can't attack {data}");
				return;
			}

			Server.Instance.StartFight(context.CurrentRoom, context.Creature, lookContext.Creature);
		}
	}
}
