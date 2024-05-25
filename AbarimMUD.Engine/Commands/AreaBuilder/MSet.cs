namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			string idStr, cmd;
			int id;
			data.ParseCommand(out idStr, out cmd);
			if (string.IsNullOrEmpty(data) || !int.TryParse(idStr, out id))
			{
				context.Send("Usage: mset _id_ name|desc|short|long|ac _params_");
				return;
			}

			var area = context.CurrentArea;
			var mobileInfo = area.GetMobileById(id);
			if (mobileInfo == null)
			{
				context.Send(string.Format("Unable to find mobile info with id {0}", id));
				return;
			}

			string cmdText, cmdData;
			cmd.ParseCommand(out cmdText, out cmdData);
			if (cmdText == "name")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset name _data_");
					return;
				}

				mobileInfo.Name = cmdData;
				Database.Areas.Update(area);
				context.SendTextLine($"Changed {mobileInfo.Id}'s name to {mobileInfo.Name}");
			}
			else if (cmdText == "desc")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset desc _data_");
					return;
				}

				mobileInfo.Description = cmdData;
				Database.Areas.Update(area);
				context.SendTextLine($"Changed {mobileInfo.Id}'s desc to {mobileInfo.Description}");
			}
			else if (cmdText == "short")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset short _data_");
					return;
				}

				mobileInfo.ShortDescription = cmdData;
				Database.Areas.Update(area);
				context.SendTextLine($"Changed {mobileInfo.Id}'s short to '{mobileInfo.ShortDescription}'");
			}
			else if (cmdText == "long")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset long _data_");
					return;
				}

				mobileInfo.LongDescription = cmdData;
				Database.Areas.Update(area);
				context.SendTextLine($"Changed {mobileInfo.Id}'s long to '{mobileInfo.LongDescription}'");
			}
			else if (cmdText == "ac")
			{
				int armorClass;
				if (string.IsNullOrEmpty(cmdData) || !int.TryParse(cmdData, out armorClass))
				{
					context.Send("Usage: mset ac _armorClass_");
					return;
				}

				mobileInfo.ArmorClass = armorClass;
				Database.Areas.Update(area);
				context.SendTextLine($"Changed {mobileInfo.Id}'s ac to '{mobileInfo.ArmorClass}'");
			}
			else
			{
				context.Send(string.Format("Unknown mset command '{0}'", cmdData));
			}
		}
	}
}