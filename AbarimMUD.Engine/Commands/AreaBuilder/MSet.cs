using AbarimMUD.Data;

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

			string cmdText, cmdData;
			cmd.ParseCommand(out cmdText, out cmdData);
			if (string.IsNullOrEmpty(data) || !int.TryParse(idStr, out id) || string.IsNullOrEmpty(cmdData))
			{
				context.Send("Usage: mset _mobileId_ name|desc|short|long|race|class|level _params_");
				return;
			}

			var mobile = Area.GetMobileById(id);
			if (mobile == null)
			{
				context.Send(string.Format("Unable to find mobile info with id {0}", id));
				return;
			}

			if (cmdText == "name")
			{
				mobile.Name = cmdData;
				context.SendTextLine($"Changed {mobile.Id}'s name to {mobile.Name}");
			}
			else if (cmdText == "desc")
			{
				mobile.Description = cmdData;
				context.SendTextLine($"Changed {mobile.Id}'s desc to {mobile.Description}");
			}
			else if (cmdText == "short")
			{
				mobile.ShortDescription = cmdData;
				context.SendTextLine($"Changed {mobile.Id}'s short to '{mobile.ShortDescription}'");
			}
			else if (cmdText == "long")
			{
				mobile.LongDescription = cmdData;
				context.SendTextLine($"Changed {mobile.Id}'s long to '{mobile.LongDescription}'");
			}
			else if (cmdText == "race")
			{
				var race = context.EnsureRace(cmdData);
				if (race == null)
				{
					return;
				}

				mobile.Race = race;
				context.SendTextLine($"Changed {mobile.Id}'s race to '{race}'");
			}
			else if (cmdText == "class")
			{
				var cls = context.EnsureClass(cmdData);
				if (cls == null)
				{
					return;
				}

				mobile.Class = cls;
				context.SendTextLine($"Changed {mobile.Id}'s class to '{cls}'");
			}
			else if (cmdText == "level")
			{
				int newLevel;
				if (!int.TryParse(cmdData, out newLevel) || newLevel < 1)
				{
					context.SendTextLine($"Can't parse level {cmdData}");
					return;
				}

				mobile.Level = newLevel;
				context.SendTextLine($"Changed {mobile.Id}'s level to '{newLevel}'");
			}
			else
			{
				context.Send(string.Format("Unknown mset command '{0}'", cmdData));
				return;
			}

			mobile.Area.Save();

			foreach (var creature in Creature.AllCreatures)
			{
				var mobileInstance = creature as MobileInstance;
				if (mobileInstance == null || mobileInstance.Info.Id != mobile.Id)
				{
					continue;
				}

				mobileInstance.InvalidateStats();
				mobileInstance.Restore();
			}
		}
	}
}