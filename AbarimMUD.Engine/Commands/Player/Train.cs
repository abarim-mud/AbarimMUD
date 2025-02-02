namespace AbarimMUD.Commands.Player
{
	public class Train: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.Room.Mobiles.Count == 0)
			{
				context.Send("Noone can train you here.");
				return false;
			}

			Tell.Execute((ExecutionContext)context.Room.Mobiles[0].Tag, $"{context.Creature.ShortDescription} let's train");
			return true;
		}
	}
}
