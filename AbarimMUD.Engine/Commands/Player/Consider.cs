namespace AbarimMUD.Commands.Player
{
	public class Consider : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Consider who?");
				return false;
			}

			var target = context.Room.Find(data);
			if (target == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return false;
			}

			if (target == context)
			{
				context.Send("Easy! Very easy indeed!");
				return false;
			}

			var diff = target.Level - context.Level;
			if (diff <= -10)
			{
				context.Send("Now where did that chicken go?");
			}
			else if (diff <= -5)
			{
				context.Send("You could it with a needle!");
			}
			else if (diff <= -2)
			{
				context.Send("Easy.");
			}
			else if (diff <= -1)
			{
				context.Send("Fairly easy.");
			}
			else if (diff == 0)
			{
				context.Send("The perfect match!");
			}
			else if (diff <= 1)
			{
				context.Send("You would need some luck!");
			}
			else if (diff <= 2)
			{
				context.Send("You would need a lot of luck!");
			}
			else if (diff <= 3)
			{
				context.Send("You would need a lot of luck and great equipment!");
			}
			else if (diff <= 5)
			{
				context.Send("Do you feel lucky, punk?");
			}
			else if (diff <= 10)
			{
				context.Send("Are you mad!?");
			}
			else
			{
				context.Send("You ARE mad!");
			}

			return true;
		}
	}
}
