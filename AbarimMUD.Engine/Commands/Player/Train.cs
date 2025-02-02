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

			BaseCommand.Say.Execute((ExecutionContext)context.Room.Mobiles[0].Tag, "let's train");
			return true;
		}
	}
}
