﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class RoomCreate : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Create new room
			var newRoom = new Room
			{
				Id = Area.NextRoomId,
				Name = "Empty",
				Description = "Empty"
			};

			var area = context.CurrentArea;
			area.Rooms.Add(newRoom);
			area.Save();

			context.Send($"New room (#{newRoom.Id}) had been created for the area '{context.Room.Area.Name}'");
			Goto.Execute(context, newRoom.Id.ToString());

			return true;
		}
	}
}
