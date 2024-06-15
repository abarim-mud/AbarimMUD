namespace AbarimMUD.Commands.Builder
{
	public class Restore : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var args = data.SplitByWhitespace();
			if (args.Length < 1)
			{
				context.Send("Usage: restore _target_");
				return;
			}

			var target = args[0];
			var targetContext = context.CurrentRoom.Find(target);
			if (targetContext == null)
			{
				context.Send(string.Format("There isnt '{0}' in this room", target));
				return;
			}

			targetContext.Creature.Restore();

			context.Send($"You fully restored {targetContext.ShortDescription}.");

			if (targetContext != context)
			{
				targetContext.Send($"{context.ShortDescription} fully restored you.");
			}
		}
	}
}
