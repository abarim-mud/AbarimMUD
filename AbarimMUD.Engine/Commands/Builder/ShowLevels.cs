using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class ShowLevels : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			for (var i = 1; i <= 100; ++i)
			{
				var levelInfo = LevelInfo.GetLevelInfo(i);
				context.Send($"{i}: Xp={levelInfo.Experience.FormatBigNumber()}, Hp={character.Class.HitpointsRange.CalculateValue(i)}, Mana={character.Class.ManaRange.CalculateValue(i)}, Moves={character.Class.MovesRange.CalculateValue(i)}");
			}

			return true;
		}
	}
}
