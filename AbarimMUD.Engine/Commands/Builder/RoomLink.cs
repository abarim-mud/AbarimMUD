using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.Builder
{
	public sealed class RoomLink : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 2)
			{
				context.Send("Usage: roomlink east|west|south|north|up|down _roomId_");
				return;
			}

			var exit = parts[0];
			Direction exitType;
			if (!Enum.TryParse(exit.CasedName(), out exitType))
			{
				context.Send(string.Format("Unable to resolve exit {0}", exit));
				return;
			}

			int id;
			if (!int.TryParse(parts[1], out id))
			{
				context.Send(string.Format("Unable to resolve id {0}", parts[1]));
				return;
			}

			var sourceRoom = context.CurrentRoom;
			if (sourceRoom.Id == id)
			{
				context.Send("You can't link the room to itself");
				return;
			}

			var destRoom = Room.GetRoomById(id);
			if (destRoom == null)
			{
				context.Send(string.Format("Could not find room with id {0}", parts[1]));
				return;
			}

			sourceRoom.ConnectRoom(destRoom, exitType);

			context.Send(string.Format("Linked the room {0} exit to {1} (#{2})",
				exitType.ToString(), destRoom.Name, id));
		}
	}
}