using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RCreate : AreaBuilderCommand
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

			new Goto().Execute(context, newRoom.Id.ToString());
		}
	}
}