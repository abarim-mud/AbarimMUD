using System.Linq;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Move : PlayerCommand
	{
		private readonly Direction _dir;

		public Move(Direction dir)
		{
			_dir = dir;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sourceRoom = context.Room;

			RoomExit exit;
			if (!sourceRoom.Exits.TryGetValue(_dir, out exit) || exit.TargetRoom == null)
			{
				context.Send("Alas, you cannot go that way...");
				return;
			}

			if (context.State.Moves <= 0)
			{
				context.Send("You're too tired to move.");
				return;
			}

			// Notify inhabits of the source room about the leave
			var dirName = _dir.GetName();
			foreach (var t in context.AllExceptMeInRoom())
			{
				t.Send(string.Format("{0} leaves {1}.", context.ShortDescription, dirName));
			}

			// Perform the movement
			context.Room = exit.TargetRoom;
			--context.State.Moves;
			
			// Notify inhabits of the destination room about the arrival
			dirName = _dir.GetOppositeDirection().GetName();

			var allExceptMeInRoom = context.AllExceptMeInRoom();
			foreach (var t in allExceptMeInRoom)
			{
				t.Send(string.Format("{0} arrives from {1}.", context.ShortDescription, dirName));
			}

			Look.Execute(context);
		}
	}
}