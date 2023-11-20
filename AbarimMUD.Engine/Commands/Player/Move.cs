﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Move : PlayerCommand
	{
		private readonly DirectionType _dir;

		public Move(DirectionType dir)
		{
			_dir = dir;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sourceRoom = context.CurrentRoom;

			var targetRoom = sourceRoom.GetConnectedRoom(_dir);
			if (targetRoom == null)
			{
				context.Send("Alas, you cannot go that way...");
				return;
			}

			// Notify inhabits of the source room about the leave
			var dirName = _dir.GetName();
			foreach (var t in context.AllExceptMeInRoom())
			{
				t.SendTextLine(string.Format("{0} leaves {1}.", context.Name, dirName));
			}

			// Notify inhabits of the destination room about the arrival
			dirName = _dir.GetOppositeDirection().GetName();

			var allExceptMeInRoom = context.AllExceptMeInRoom();
			foreach (var t in allExceptMeInRoom)
			{
				t.SendTextLine(string.Format("{0} arrives from {1}.", context.Name, dirName));
			}

			context.Logger.Info("Moved to room with id {0}", targetRoom.Id);

			new Look().Execute(context, string.Empty);
		}
	}
}