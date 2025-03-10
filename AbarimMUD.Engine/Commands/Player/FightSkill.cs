using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class FightSkill : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Only players can set the fightskill.");
				return false;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: fightskill _skillName_|off");
				return false;
			}

			if (data.EqualsToIgnoreCase("off"))
			{
				character.FightSkill = string.Empty;
				character.Save();

				context.Send("Cleared the current fightskill.");
				return true;
			}

			var command = FindCommand(data);
			if (command == null || command.RequiredRole > context.Role)
			{
				context.Send($"Unknown ability '{data}'.");
				return false;
			}

			if (!command.CanFightskill)
			{
				context.Send($"Skill {command.Name} couldn't be set to auto.");
				return false;
			}

			character.FightSkill = command.Name;
			character.Save();
			context.Send($"fightskill was set to {command.Name}.");

			return true;
		}
	}
}
