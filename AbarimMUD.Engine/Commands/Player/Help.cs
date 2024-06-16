namespace AbarimMUD.Commands.Player
{
	public sealed class Help : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var count = 0;
			foreach (var ac in AllCommands)
			{
				if (ac.Value.RequiredType <= context.Role)
				{
					context.Send(ac.Key);
					++count;
				}
			}

			context.Send($"Total commands: {count}");
		}
	}
}
