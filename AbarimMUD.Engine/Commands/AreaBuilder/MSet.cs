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
				context.Send("Usage: mset _id_ name|desc|short|long _params_");
				return;
			}

			var mobileInfo = Database.MobileInfos.GetById(id);
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
				Database.MobileInfos.Update(mobileInfo);
				context.SendTextLine(string.Format("Changed {0}'s name to {1}", mobileInfo.Id, mobileInfo.Name));
			}
			else if (cmdText == "desc")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset desc _data_");
					return;
				}

				mobileInfo.Description = cmdData;
				Database.MobileInfos.Update(mobileInfo);
				context.SendTextLine(string.Format("Changed {0}'s desc to {1}", mobileInfo.Id, mobileInfo.Description));
			}
			else if (cmdText == "short")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset short _data_");
					return;
				}

				mobileInfo.ShortDescription = cmdData;
				Database.MobileInfos.Update(mobileInfo);
				context.SendTextLine(string.Format("Changed {0}'s short to '{1}'", mobileInfo.Id, mobileInfo.ShortDescription));
			}
			else if (cmdText == "long")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset long _data_");
					return;
				}

				mobileInfo.LongDescription = cmdData;
				Database.MobileInfos.Update(mobileInfo);
				context.SendTextLine(string.Format("Changed {0}'s long to '{1}'", mobileInfo.Id, mobileInfo.LongDescription));
			}
			else
			{
				context.Send(string.Format("Unknown mset command '{0}'", cmdData));
			}
		}
	}
}