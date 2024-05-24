using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Help : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sd = new StringBuilder();

			foreach (var ac in AllCommands)
			{
				if (ac.Value.RequiredType <= context.Role)
				{
					sd.AddTextLine(ac.Key);
				}
			}

			context.Send(sd.ToString());
		}
	}
}
