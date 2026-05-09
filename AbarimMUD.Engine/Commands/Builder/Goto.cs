using AbarimMUD.Commands.Player;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Builder
{
	public sealed class Goto : BuilderCommand
	{
		private static bool TeleportTo(ExecutionContext context, Room newRoom)
		{
			var sourceRoom = context.Room;

			if (sourceRoom != null)
			{
				context.SendRoomExceptMe(string.Format("{0} disappears suddenly.", context.ShortDescription));
			}

			context.Room = newRoom;

			var msg = string.Format("{0} appears from thin air.", context.ShortDescription);
			foreach (var ctx in context.AllInRoom())
			{
				if (ctx != context)
				{
					ctx.Send(msg);
				}
			}

			context.Send("You had been transfered!");
			Look.Execute(context);
			return true;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrWhiteSpace(data))
			{
				context.Send("Usage: goto _roomId_ | _playerName_ | _mobKeyword_");
				return false;
			}

			// Try room id first
			int id;
			if (int.TryParse(data, out id))
			{
				var newRoom = Room.GetRoomById(id);
				if (newRoom != null)
				{
					return TeleportTo(context, newRoom);
				}

				context.Send(string.Format("Unable to find room with id {0}", id));
				return false;
			}

			// Try online player by name
			var player = Character.ActiveCharacters.FirstOrDefault(c => c.Name.EqualsToIgnoreCase(data));
			if (player != null)
			{
				return TeleportTo(context, player.Room);
			}

			// Try mobile instance by keyword across all areas
			foreach (var creature in Creature.ActiveCreatures)
			{
				var mob = creature as MobileInstance;
				if (mob == null || mob.Room == null)
				{
					continue;
				}

				if (mob.MatchesKeyword(data))
				{
					return TeleportTo(context, mob.Room);
				}
			}

			context.Send(string.Format("No room, player, or mobile found matching '{0}'", data));
			return false;
		}
	}
}