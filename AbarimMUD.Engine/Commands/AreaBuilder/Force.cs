﻿using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class Force : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var args = data.SplitByWhitespace(2);
			if (args.Length < 2)
			{
				context.Send("Usage: force _target_ _command_");
				return;
			}

			var target = args[0];
			var command = args[1];
			var targetContext = context.CurrentRoom.Find(target);
			if (targetContext == null)
			{
				context.Send(string.Format("There isnt '{0}' in this room", target));
				return;
			}

			if (targetContext != context)
			{
				targetContext.Send(string.Format("{0} forces you to '{1}'.", context.ShortDescription, command));
			}

			targetContext.ParseAndExecute(command);
		}
	}
}