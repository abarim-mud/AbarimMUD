using AbarimMUD.Data;
using System.Text;

namespace AbarimMUD.Commands.Builder
{
	public class ShowLevels : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var sb = new StringBuilder();

			var character = context.Creature as Character;
			for (var i = 1; i <= 100; ++i)
			{
				var levelInfo = LevelInfo.GetLevelInfo(i);
				sb.AppendLine($"{i}: Xp={levelInfo.Experience.FormatBigNumber()}, Hp={character.Class.HitpointsRange.CalculateValue(i)}");
			}

			context.Send(sb.ToString());
		}
	}
}
