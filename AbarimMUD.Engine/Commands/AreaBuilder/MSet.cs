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
				context.Send("Usage: mset _mobileId_ name|desc|short|long|ac _params_");
				return;
			}

			var mobile = Database.GetMobileById(id);
			if (mobile == null)
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

				mobile.Name = cmdData;
				Database.Update(mobile);
				context.SendTextLine($"Changed {mobile.Id}'s name to {mobile.Name}");
			}
			else if (cmdText == "desc")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset desc _data_");
					return;
				}

				mobile.Description = cmdData;
				Database.Update(mobile);
				context.SendTextLine($"Changed {mobile.Id}'s desc to {mobile.Description}");
			}
			else if (cmdText == "short")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset short _data_");
					return;
				}

				mobile.ShortDescription = cmdData;
				Database.Update(mobile);
				context.SendTextLine($"Changed {mobile.Id}'s short to '{mobile.ShortDescription}'");
			}
			else if (cmdText == "long")
			{
				if (string.IsNullOrEmpty(cmdData))
				{
					context.Send("Usage: mset long _data_");
					return;
				}

				mobile.LongDescription = cmdData;
				Database.Update(mobile);
				context.SendTextLine($"Changed {mobile.Id}'s long to '{mobile.LongDescription}'");
			}
			else if (cmdText == "ac")
			{
				int armorClass;
				if (string.IsNullOrEmpty(cmdData) || !int.TryParse(cmdData, out armorClass))
				{
					context.Send("Usage: mset ac _armorClass_");
					return;
				}

				mobile.ArmorClass = armorClass;
				Database.Update(mobile);
				context.SendTextLine($"Changed {mobile.Id}'s ac to '{mobile.ArmorClass}'");
			}
			else
			{
				context.Send(string.Format("Unknown mset command '{0}'", cmdData));
			}
		}
	}
}