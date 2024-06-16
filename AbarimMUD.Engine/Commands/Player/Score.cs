using AbarimMUD.Data;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Score : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			sb.AppendLine($"You are {context.ShortDescription}, {context.Creature.Class.Name} of level {context.Creature.Level}.");
			var asCharacter = context.Creature as Character;
			if (asCharacter != null)
			{
				if (asCharacter.Level < Configuration.MaximumLevel)
				{
					var nextLevelInfo = LevelInfo.GetLevelInfo(asCharacter.Level + 1);
					sb.AppendLine($"Experience: {asCharacter.Experience.FormatBigNumber()}/{nextLevelInfo.Experience.FormatBigNumber()}");
				}
				else
				{
					sb.AppendLine($"Experience: {asCharacter.Experience.FormatBigNumber()}");
				}
				sb.AppendLine($"Gold: {asCharacter.Wealth.FormatBigNumber()}");
			}

			var stats = context.Creature.Stats;
			var state = context.Creature.State;
			sb.AppendLine($"Hitpoints: {state.Hitpoints}/{stats.MaxHitpoints} + {stats.HitpointsRegen}");
			sb.AppendLine("Armor: " + stats.Armor);
			for (var i = 0; i < stats.Attacks.Count; i++)
			{
				var attack = stats.Attacks[i];
				sb.AppendLine($"Attack #{i + 1}: {attack}");
			}

			context.Send(sb.ToString());
		}
	}
}
