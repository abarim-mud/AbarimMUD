using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RLinkClear : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var exit = data.Trim().ToLower();

			if (string.IsNullOrEmpty(exit))
			{
				context.Send("Usage: rlinkclear east|west|south|north|up|down");
				return;
			}

			DirectionType exitType;
			if (!Enum.TryParse(exit, out exitType))
			{
				context.Send(string.Format("Unable to resolve exit {0}", exit));
				return;
			}

			var sourceRoom = context.CurrentRoom;
			var targetRoom = sourceRoom.GetConnectedRoom(exitType);
			if (targetRoom == null)
			{
				context.Send(string.Format("The room isnt connected to anything at the direction {0}", exitType.ToString()));
				return;
			}

			sourceRoom.Connect(exitType, null);
			Database.Rooms.Update(sourceRoom);

			var destDir = exitType.GetOppositeDirection();
			targetRoom.Connect(destDir, null);
			Database.Rooms.Update(targetRoom);

			context.Send(string.Format("Cleared the link from the room {0} exit to {1} (#{2})",
				exitType, targetRoom.Name, targetRoom.Id));
		}
	}
}