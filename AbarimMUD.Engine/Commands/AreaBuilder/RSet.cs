namespace AbarimMUD.Commands.AreaBuilder
{
	public class RSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			string cmdText, cmdData;
			data.ParseCommand(out cmdText, out cmdData);
			if (string.IsNullOrEmpty(data) ||
				(cmdText != "name" &&
				cmdText != "desc"))
			{
				context.Send("Usage: rset name|desc _params_");
				return;
			}

			var room = context.CurrentRoom;
			var doSave = true;
			switch(cmdText)
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