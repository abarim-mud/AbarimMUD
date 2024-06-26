﻿using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Gossip : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sd = new StringBuilder();

			sd.Append("[magenta]");
			sd.AppendLine(string.Format("You gossip-- '{0}'", data));
			sd.Append("[reset]");
			context.Send(sd.ToString());

			// Show it to others
			sd.Clear();
			sd.AppendLine();
			sd.Append("[magenta]");
			sd.AppendLine(string.Format("{0} gossips-- '{1}'", context.ShortDescription, data));
			sd.Append("[reset]");

			foreach (var s in context.AllExceptMe())
			{
				s.Send(sd.ToString());
			}

			return true;
		}
	}
}