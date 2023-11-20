namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class Force : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			string target, command;
			data.ParseCommand(out target, out command);
			if (string.IsNullOrEmpty(target) || string.IsNullOrEmpty(command))
			{
				context.SendTextLine("Usage: force _target_ _command_");
				return;
			}
			else
			{
				var targetContext = context.CurrentRoom.Find(target);
				if (targetContext == null)
				{
					context.SendTextLine(string.Format("There isnt '{0}' in this room", data));
					return;
				}

				if (targetContext != context)
				{
					targetContext.SendTextLine(string.Format("{0} forces you to '{1}'.", context.Name, command));
				}

				ParseAndExecute(targetContext, command);
			}
		}
	}
}