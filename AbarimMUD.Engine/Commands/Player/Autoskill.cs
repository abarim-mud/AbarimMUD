using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Autoskill : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Only players can set the autoskill.");
				return false;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: autoskill _skillName_|off");
				return false;
			}

			if (data.EqualsToIgnoreCase("off"))
			{
				character.Autoskill = string.Empty;
				character.Save();

				context.Send("Cleared the current autoskill.");
				return true;
			}

			var command = FindCommand(data);
			if (command == null || command.RequiredType > context.Role)
			{
				context.Send($"Unknown skill '{data}'.");
				return false;
			}

			if (!command.CanAutoskill)
			{
				context.Send($"Skill {command.Name} couldn't be set to auto.");
				return false;
			}

			character.Autoskill = command.Name;
			character.Save();
			context.Send($"Autoskill was set to {command.Name}.");

			return true;
		}
	}
}
