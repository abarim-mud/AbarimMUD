using AbarimMUD.Commands.Player;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public sealed class Goto : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			int id;
			if (!int.TryParse(data, out id))
			{
				context.Send("Usage: goto _roomId_");
				return;
			}

			var newRoom = Room.GetRoomById(id);
			if (newRoom == null)
			{
				context.Send(string.Format("Unable to find room with id {0}", id));
				return;
			}

			context.CurrentRoom = newRoom;
			context.Send("You had been transfered!");

			new Look().Execute(context);
		}
	}
}