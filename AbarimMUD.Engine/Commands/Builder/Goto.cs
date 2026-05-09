using AbarimMUD.Commands.Player;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Builder
{
	public sealed class Goto : BuilderCommand
	{
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
					context.Room = newRoom;
					context.Send("You had been transfered!");
					Look.Execute(context);
					return true;
				}

				context.Send(string.Format("Unable to find room with id {0}", id));
				return false;
			}

			// Try online player by name
			var player = Character.ActiveCharacters.FirstOrDefault(c => c.Name.EqualsToIgnoreCase(data));
			if (player != null)
			{
				context.Room = player.Room;
				context.Send("You had been transfered!");
				Look.Execute(context);
				return true;
			}

			// Try mobile instance by keyword across all areas
			foreach (var area in Area.Storage)
			{
				foreach (var room in area.Rooms)
				{
					foreach (var mob in room.Mobiles)
					{
						if (mob.MatchesKeyword(data))
						{
							context.Room = room;
							context.Send("You had been transfered!");
							Look.Execute(context);
							return true;
						}
					}
				}
			}

			context.Send(string.Format("No room, player, or mobile found matching '{0}'", data));
			return false;
		}
	}
}