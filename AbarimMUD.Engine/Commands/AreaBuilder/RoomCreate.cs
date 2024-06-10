using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class RoomCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
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

			context.SendTextLine($"New room (#{newRoom.Id}) had been created for the area '{context.CurrentRoom.Area.Name}'");
			Goto.Execute(context, newRoom.Id.ToString());
		}
	}
}
