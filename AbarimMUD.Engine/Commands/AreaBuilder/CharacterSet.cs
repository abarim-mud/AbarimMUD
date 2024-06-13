using AbarimMUD.Data;
using System.Xml.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class CharacterSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(3);
			if (parts.Length < 3)
			{
				context.Send("Usage: characterset name|desc|race|class|level _characterName_ _params_");
				return;
			}

			var name = parts[1];
			var character = context.EnsureCharacterByName(name);
			if (character == null)
			{
				return;
			}

			var builderCharacter = (Character)context.Creature;
			if (context.Role == Role.Builder && character.Account.Name != builderCharacter.Account.Name)
			{
				context.Send("A builder can only change their own characters.");
				return;
			}

			var cmdText = parts[0].ToLower();
			var cmdData = parts[2];
			switch (cmdText)
			{
				case "name":
					{
						var existing = Character.GetCharacterByName(cmdData);
						if (existing != null)
						{
							context.Send($"Character with name {cmdData} exists already.");
							break;
						}

						character.Name = cmdData;
						context.Send($"Changed character name to '{cmdData}'");

						var session = Session.FindSessionByCharacterName(name);
						if (session != null)
						{
							session.OnCharacterNameChanged();
							session.Context.Send($"Your name was changed to '{cmdData}'.");
						}

					}
					break;

				case "desc":
					{
						character.PlayerDescription = cmdData;
						context.Send($"Changed {character.Name}'s desc to '{character.Description}'");

						var session = Session.FindSessionByCharacterName(name);
						if (session != null)
						{
							session.Context.Send($"Your description was changed to '{cmdData}'");
						}
					}
					break;

				case "race":
					{
						var race = context.EnsureRace(cmdData);
						if (race == null)
						{
							return;
						}

						character.PlayerRace = race;
						context.Send($"Changed {character.Name}'s race to '{race}'");

						var session = Session.FindSessionByCharacterName(name);
						if (session != null)
						{
							session.Context.Send($"Your race was changed to '{race.Name}'");
						}
					}
					break;

				case "class":
					{
						var cls = context.EnsureClass(cmdData);
						if (cls == null)
						{
							return;
						}

						character.PlayerClass = cls;
						context.Send($"Changed {character.Name}'s class to '{cls}'");

						var session = Session.FindSessionByCharacterName(name);
						if (session != null)
						{
							session.Context.Send($"Your class was changed to '{cls.Name}'");
						}
					}
					break;

				case "level":
					{
						int newLevel;
						if (!int.TryParse(cmdData, out newLevel) || newLevel < 1)
						{
							context.Send($"Can't parse level {cmdData}");
							return;
						}

						character.PlayerLevel = newLevel;
						context.Send($"Changed {character.Name}'s level to '{newLevel}'");

						var session = Session.FindSessionByCharacterName(name);
						if (session != null)
						{
							session.Context.Send($"Your level was changed to '{newLevel}'");
						}
					}
					break;

				default:
					{
						context.Send($"Unknown character property '{cmdText}'");
						return;
					}
			}

			character.Restore();
			character.Save();
		}
	}
}
