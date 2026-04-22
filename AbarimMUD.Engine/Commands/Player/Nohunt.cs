namespace AbarimMUD.Commands.Player
{
	public class Nohunt : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			if (!context.HuntInfo.IsActive)
			{
				context.Send("You aren't hunting.");
				return false;
			}

			context.BreakHunt();
			return true;
		}
	}
}
