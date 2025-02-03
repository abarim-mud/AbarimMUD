using AbarimMUD.Data;

namespace AbarimMUD.Commands.Administrator
{
	public class Award: AdministratorCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 3)
			{
				context.Send($"Usage: award xp|gold|sp _amount_ _character_");
				return false;
			}

			var objectType = parts[0].ToLower();
			if (objectType != "xp" && objectType != "gold" && objectType != "sp")
			{
				context.Send($"Couldn't award '{objectType}'");
				return false;
			}

			int amount;
			if (!int.TryParse(parts[1], out amount))
			{
				context.Send($"Unable to parse amount '{parts[1]}'");
				return false;
			}

			var character = Character.GetCharacterByName(parts[2]);
			if (character == null)
			{
				context.Send($"Unable to find character '{parts[2]}'");
				return false;
			}

			var characterContext = (ExecutionContext)character.Tag;
			if (characterContext == null)
			{
				context.Send($"Character {character.Name} isn't online");
				return false;
			}

			characterContext.Send($"{context.ShortDescription} awarded you {amount} of {parts[0]}.");
			context.Send($"You awarded {character.Name} {amount} of {parts[0]}.");

			switch (parts[0])
			{
				case "xp":
					characterContext.AwardXp(amount);
					break;
				case "gold":
					character.Gold += amount;
					character.Save();
					break;
				case "sp":
					character.SkillPoints += amount;
					character.Save();
					break;
			}

			return true;
		}
	}
}
