using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MobileSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(3);
			if (parts.Length < 3)
			{
				context.Send("Usage: mobileset keywords|short|long|desc|race|class|level _mobileId_ _params_");
				return;
			}

			var id = parts[1];
			var mobile = Mobile.GetMobileById(id);
			if (mobile == null)
			{
				context.Send(string.Format("Unable to find mobile with id {0}", id));
				return;
			}

			var cmdText = parts[0].ToLower();
			var cmdData = parts[2];
			switch (cmdText)
			{
				case "keywords":
					{
						mobile.Keywords = cmdData.SplitByWhitespace().ToHashSet();
						context.SendTextLine($"Changed {mobile.Id}'s name to '{mobile.Keywords.JoinKeywords()}'");
					}
					break;

				case "desc":
					{
						mobile.Description = cmdData;
						context.SendTextLine($"Changed {mobile.Id}'s desc to '{mobile.Description}'");
					}
					break;

				case "short":
					{
						mobile.ShortDescription = cmdData;
						context.SendTextLine($"Changed {mobile.Id}'s short to '{mobile.ShortDescription}'");
					}
					break;

				case "long":
					{
						mobile.LongDescription = cmdData;
						context.SendTextLine($"Changed {mobile.Id}'s long to '{mobile.LongDescription}'");
					}
					break;

				case "race":
					{
						var race = context.EnsureRace(cmdData);
						if (race == null)
						{
							return;
						}

						mobile.Race = race;
						context.SendTextLine($"Changed {mobile.Id}'s race to '{race}'");
					}
					break;

				case "class":
					{
						var cls = context.EnsureClass(cmdData);
						if (cls == null)
						{
							return;
						}

						mobile.Class = cls;
						context.SendTextLine($"Changed {mobile.Id}'s class to '{cls}'");
					}
					break;

				case "level":
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
					break;

				default:
					{
						context.Send(string.Format("Unknown mobile property '{0}'", cmdData));
						return;
					}
			}

			mobile.Save();

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