using AbarimMUD.Commands.Player;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class Goto : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			int newRoomId;
			if (!int.TryParse(data, out newRoomId))
			{
				context.Send("Usage: goto roomId");
				return;
			}

			var newRoom = Database.Rooms.GetById(newRoomId);
			if (newRoom == null)
			{
				context.Send(string.Format("Unable to find room with id {0}", newRoomId));
				return;
			}

			context.CurrentRoom = newRoom;
			context.SendTextLine("You had been transfered!");

			new Look().Execute(context, string.Empty);
		}
	}
}