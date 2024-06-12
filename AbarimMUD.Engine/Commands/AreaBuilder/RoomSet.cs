using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class RoomSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(2);
			if (parts.Length < 2)
			{
				context.Send("Usage: roomset name|desc _params_");
				return;
			}

			var room = context.CurrentRoom;
			var doSave = true;

			var cmdText = parts[0];
			var cmdData = parts[1];
			switch (cmdText)
			{
				case "name":
					{
						room.Name = cmdData.Trim();
						context.Send(string.Format("Changed {0}'s name to {1}", room.Id, room.Name));
					}
					break;

				case "desc":
					{
						room.Description = cmdData.Trim();
						context.Send(string.Format("Changed {0}'s description to {1}", room.Id, room.Name));
					}
					break;

				default:
					context.Send($"Unknown room property {cmdText}");
					doSave = false;
					break;
			}

			if (doSave)
			{
				room.Area.Save();
			}
		}
	}
}