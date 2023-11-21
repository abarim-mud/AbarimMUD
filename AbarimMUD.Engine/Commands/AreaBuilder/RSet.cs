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
				cmdText != "name" ||
				cmdText != "desc")
			{
				context.Send("Usage: rset name|desc _params_");
				return;
			}

			if (cmdText == "name")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: rset name _data_");
					return;
				}
				var room = context.CurrentRoom;
				room.Name = cmdData.Trim();
				Database.Update(room);
				context.SendTextLine(string.Format("Changed {0}'s name to {1}", room.Id, room.Name));
			}
		}
	}
}