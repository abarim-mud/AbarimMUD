using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var area = context.CurrentArea;

			// Create new room
			var newRoom = new Room
			{
				Id = Database.NewRoomId,
				Name = "Empty",
				Description = "Empty"
			};

			context.CurrentRoom.Area.Rooms.Add(newRoom);
			Database.Update(newRoom);

			context.SendTextLine(string.Format("New room (#{0}) had been created for the area {1} (#{2})",
				newRoom.Id,
				context.CurrentRoom.Area.Name,
				context.CurrentRoom.Area.Id));

			new Goto().Execute(context, newRoom.Id.ToString());
		}
	}
}