using AbarimMUD.Data;
using System.Text;

namespace AbarimMUD.Commands.Builder
{
	public class Slain: BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Usage: slain _target_");
				return;
			}

			var lookContext = context.CurrentRoom.Find(data);
			if (lookContext == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return;
			}

			lookContext.Creature.Slain();

			var sb = new StringBuilder();
			sb.AppendLine($"{lookContext.Creature.ShortDescription} is dead! R.I.P.");

			var character = context.Creature as Character;

			var targetMobile = lookContext.Creature as MobileInstance;
			if (character != null && targetMobile != null)
			{
				var xpAward = targetMobile.Stats.XpAward;
				character.Experience += xpAward;
				sb.AppendLine($"Total exp for kill is {xpAward.FormatBigNumber()}.");

				character.Wealth += targetMobile.Info.Wealth;
				sb.AppendLine($"You get {targetMobile.Info.Wealth.FormatBigNumber()} from the corpse of {targetMobile.ShortDescription}.");

				character.Save();
			}

			context.Send(sb.ToString());
		}
	}
}
