﻿using System.Text;

namespace AbarimMUD.Commands.Player
{
	public sealed class Say : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			sb.Append("[magenta]");
			sb.AppendLine(string.Format("You say '{0}'", data));
			sb.Append("[reset]");
			context.Send(sb.ToString());

			// Show it to others
			sb.Clear();
			sb.Append("[magenta]");
			sb.AppendLine(string.Format("{0} says '{1}'", context.ShortDescription, data));
			sb.Append("[reset]");

			context.SendRoomExceptMe(sb.ToString());

			return true;
		}
	}
}