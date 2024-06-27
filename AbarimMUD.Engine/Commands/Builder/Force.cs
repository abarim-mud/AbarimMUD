namespace AbarimMUD.Commands.Builder
{
	public sealed class Force : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var args = data.SplitByWhitespace(2);
			if (args.Length < 2)
			{
				context.Send("Usage: force _target_ _command_");
				return false;
			}

			var target = args[0];
			var command = args[1];
			var targetContext = context.Room.Find(target);
			if (targetContext == null)
			{
				context.Send(string.Format("There isnt '{0}' in this room", target));
				return false;
			}

			if (targetContext != context)
			{
				targetContext.Send(string.Format("{0} forces you to '{1}'.", context.ShortDescription, command));
			}

			targetContext.ParseAndExecute(command);

			return true;
		}
	}
}