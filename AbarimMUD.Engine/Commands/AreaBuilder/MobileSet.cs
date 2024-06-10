using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MobileSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(3);
			if (parts.Length < 3)
			{
				context.Send("Usage: mset name|desc|short|long|race|class|level _mobileId_ _params_");
				return;
			}

			int id;
			if (!int.TryParse(parts[1], out id))
			{
				context.Send($"Unable to parse mobile id {id}");
				return;
			}

			var mobile = Mobile.GetMobileById(id);
			if (mobile == null)
			{
				context.Send(string.Format("Unable to find mobile with id {0}", id));
				return;
			}

			var cmdText = parts[0];
			var cmdData = parts[2];
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
				context.Send(string.Format("Unknown mobile property '{0}'", cmdData));
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