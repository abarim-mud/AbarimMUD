﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Move : PlayerCommand
	{
		private readonly Direction _dir;

		public Move(Direction dir)
		{
			_dir = dir;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var sourceRoom = context.Room;

			RoomExit exit;
			if (!sourceRoom.Exits.TryGetValue(_dir, out exit) || exit.TargetRoom == null)
			{
				context.Send("Alas, you cannot go that way...");
				return false;
			}

			var cost = sourceRoom.GetMovementCost();
			if (context.State.Moves < cost)
			{
				context.Send("You're too tired to move.");
				context.BreakHunt();
				return false;
			}

			if (!context.SuppressStopHuntOnMovement)
			{
				context.BreakHunt();
			}

			// Notify inhabits of the source room about the leave
			var dirName = _dir.GetName();
			context.Send($"You go {dirName}.");
			context.SendRoomExceptMe(string.Format("{0} leaves {1}.", context.ShortDescription, dirName));

			// Perform the movement
			context.Room = exit.TargetRoom;
			context.State.Moves -= cost;

			// Notify inhabits of the destination room about the arrival
			dirName = _dir.GetOppositeDirection().GetName();

			context.SendRoomExceptMe(string.Format("{0} arrives from {1}.", context.ShortDescription, dirName));

			Look.Execute(context);

			return true;
		}
	}
}