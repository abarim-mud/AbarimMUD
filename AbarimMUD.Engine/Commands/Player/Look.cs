using System.Text;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public sealed class Look : PlayerCommand
	{
		private string BuildRoomDescription(ExecutionContext context)
		{
			var room = context.CurrentRoom;

			var sd = new StringBuilder();
			sd.Append(ConsoleCommand.ForeColorCyan);

			var name = room.Name;
			if (context.Type >= (Role.AreaBuilder))
			{
				name += string.Format(" (#{0})", room.Id);
			}

			sd.AddTextLine(name);
			sd.Append(ConsoleCommand.ColorClear);
			sd.Append("   ");
			sd.AddTextLine(room.Description);
			sd.Append(ConsoleCommand.ForeColorCyan);
			sd.Append("Exits: ");

			var first = true;
			foreach (var exit in room.Exits)
			{
				if (!first)
				{
					sd.Append(ConsoleCommand.ColorClear);
					sd.Append(" ");
				}

				sd.Append(ConsoleCommand.ForeColorCyan);
				sd.Append(ConsoleCommand.Underline);
				sd.Append(exit);

				if (context.Type >= (Role.AreaBuilder))
				{
					sd.Append(string.Format("(#{0})", exit.TargetRoomId));
				}

				first = false;
			}

			sd.AddNewLine();
			sd.Append(ConsoleCommand.ColorClear);

			// Mobiles
			foreach (var mobile in room.Mobiles)
			{
				sd.AddTextLine(mobile.Info.ShortDescription);
			}

			// Characters
			foreach (var character in room.Characters)
			{
				var asPlayer = context as PlayerExecutionContext;
				if (asPlayer != null && asPlayer.Session.Character == character)
				{
					continue;
				}

				sd.AddTextLine(string.Format("{0} is standing here.", character.Id));
			}

			return sd.ToString();
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				// Look room
				var sd = BuildRoomDescription(context);
				context.Send(sd);
			}
			else
			{
				var lookContext = context.CurrentRoom.Find(data);

				if (lookContext == null)
				{
					context.SendTextLine(string.Format("There isnt '{0}' in this room", data));
					return;
				}

				context.SendTextLine(lookContext.Look);

				if (lookContext != context)
				{
					lookContext.SendTextLine(string.Format("{0} looks at you.", context.Name));
				}

				foreach (var t in context.AllExceptMeInRoom())
				{
					if (t != lookContext)
					{
						t.SendTextLine(string.Format("{0} looks at {1}.", context.Name, lookContext.Name));
					}
				}
			}
		}
	}
}