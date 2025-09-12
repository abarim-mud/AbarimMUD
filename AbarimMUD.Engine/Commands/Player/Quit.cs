using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Quit : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.Session == null)
			{
				return false;
			}

			context.AppendPrompt = false;
			context.Send("Bye!");

			context.Session.Disconnect();

			Commands.ExecutionContext.SendAll($"[magenta]{context.ShortDescription} left the game.[reset]");


			return true;
		}
	}
}
