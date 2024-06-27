using AbarimMUD.Combat;

namespace AbarimMUD.Commands.Builder
{
	public class Slain : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Usage: slain _target_");
				return false;
			}

			var lookContext = context.Room.Find(data);
			if (lookContext == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return false;
			}

			context.Slain(lookContext);

			return true;
		}
	}
}
