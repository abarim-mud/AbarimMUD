namespace AbarimMUD.Commands.Player
{
	public class Nohunt : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ability = context.EnsureAbility("hunt");
			if (ability == null)
			{
				return false;
			}

			if (context.HuntTarget == null)
			{
				context.Send("You aren't hunting.");
				return false;
			}

			context.BreakHunt();
			return true;
		}
	}
}
