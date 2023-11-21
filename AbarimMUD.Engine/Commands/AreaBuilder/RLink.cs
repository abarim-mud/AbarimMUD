using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RLink : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			string exit, idStr;
			data.ParseCommand(out exit, out idStr);

			if (string.IsNullOrEmpty(data) || string.IsNullOrEmpty(exit))
			{
				context.Send("Usage: rlink east|west|south|north|up|down _id_");
				return;
			}

			Direction exitType;
			if (!Enum.TryParse(exit, out exitType))
			{
				context.Send(string.Format("Unable to resolve exit {0}", exit));
				return;
			}

			int id;
			if (!int.TryParse(idStr, out id))
			{
				context.Send(string.Format("Unable to resolve id {0}", idStr));
				return;
			}

			var sourceRoom = context.CurrentRoom;
			if (sourceRoom.Id == id)
			{
				context.Send("You can't link the room to itself");
				return;
			}

			var destRoom = Database.GetRoomById(id);
			if (destRoom == null)
			{
				context.Send(string.Format("Could not find room with id {0}", idStr));
				return;
			}

			Database.ConnectRoom(sourceRoom, destRoom, exitType);

			context.Send(string.Format("Linked the room {0} exit to {1} (#{2})",
				exitType.ToString(), destRoom.Name, id));
		}
	}
}