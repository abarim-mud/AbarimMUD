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
				context.Send("Usage: rset name|desc _params_");
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
						if (string.IsNullOrEmpty(cmdData))
						{
							context.Send("Usage: rset name _data_");
							return;
						}
						room.Name = cmdData.Trim();
						context.SendTextLine(string.Format("Changed {0}'s name to {1}", room.Id, room.Name));
					}
					break;

				case "desc":
					{
						if (string.IsNullOrEmpty(cmdData))
						{
							context.Send("Usage: rset desc _data_");
							return;
						}
						room.Description = cmdData.Trim();
						context.SendTextLine(string.Format("Changed {0}'s description to {1}", room.Id, room.Name));
					}
					break;

				default:
					context.SendTextLine($"Unknown room property {cmdText}");
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