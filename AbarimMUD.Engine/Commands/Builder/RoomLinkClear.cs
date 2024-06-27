using System;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public sealed class RoomLinkClear : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var exit = data.Trim().ToLower();
			if (string.IsNullOrEmpty(exit))
			{
				context.Send("Usage: roomlinkclear east|west|south|north|up|down");
				return false;
			}

			Direction exitType;
			if (!Enum.TryParse(exit, out exitType))
			{
				context.Send(string.Format("Unable to resolve exit {0}", exit));
				return false;
			}

			var sourceRoom = context.Room;
			RoomExit roomExit;
			if (!sourceRoom.Exits.TryGetValue(exitType, out roomExit))
			{
				context.Send(string.Format("The room isnt connected to anything at the direction {0}", exitType.ToString()));
				return false;
			}

			sourceRoom.DisconnectRoom(exitType);
			if (roomExit.TargetRoom != null)
			{
				context.Send(string.Format("Cleared the link from the room {0} exit to {1} (#{2})",
					exitType, roomExit.TargetRoom.Name, roomExit.TargetRoom.Id));
			}
			else
			{
				context.Send(string.Format("Cleared the link from the room {0} exit to {1}", exitType));
			}

			return true;
		}
	}
}