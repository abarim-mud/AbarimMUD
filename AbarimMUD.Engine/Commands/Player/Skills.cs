using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Skills : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var asCharacter = context.Creature as Character;
			if (asCharacter == null)
			{
				return false;
			}

			foreach (var pair in asCharacter.Skills)
			{
				context.Send($"{pair.Value.Skill.Name}: {pair.Value.Level}");
			}

			return true;
		}
	}
}
