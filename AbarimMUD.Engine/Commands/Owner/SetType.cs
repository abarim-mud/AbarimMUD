using System.Collections.Generic;
using System.Linq;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Owner
{
	public sealed class SetType : OwnerCommand
	{
		private static readonly Dictionary<string, Role> _namesToTypes = new Dictionary<string, Role>();

		static SetType()
		{
			_namesToTypes["player"] = Role.Player;
			_namesToTypes["area"] = Role.AreaBuilder;
			_namesToTypes["world"] = Role.WorldBuilder;
			_namesToTypes["lowadmin"] = Role.LowAdministrator;
			_namesToTypes["highadmin"] = Role.HighAdministrator;
			_namesToTypes["owner"] = Role.Owner;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = (from d in data.Split(' ') select d.Trim()).ToArray();

			if (parts.Length != 2)
			{
				context.Send("Usage: settype <character> player|area|world|lowadmin|highadmin|owner");
				return;
			}

			var characterName = parts[0];

			// Find the character
			var character = Database.Characters.GetByName(characterName);
			if (character == null)
			{
				context.Send(string.Format("Could not find character '{0}'", characterName));
				return;
			}

			var typeName = parts[1];

			// Resolve the type
			Role type;
			if (!_namesToTypes.TryGetValue(typeName.ToLower(), out type))
			{
				context.Send(string.Format("Could not resolve type '{0}'", typeName));
				return;
			}

			if (character.Role == type)
			{
				context.Send(string.Format("{0}'s type is {1} already.", character.Id, type.ToString()));
				return;
			}

			var oldType = character.Role;

			// Change
			character.Role = type;
			Database.Characters.Update(character);

			context.SendTextLine(string.Format("Changed {0}'s type from {1} to {2}", character.Id, oldType.ToString(),
				type.ToString()));

			// Check if it's online
			foreach (var s in Server.Instance.Sessions)
			{
				if (s.Character != null && s.Character.Id == character.Id)
				{
					// It's online
					// Inform him or her that the type had been changed
					s.Context.SendTextLine(string.Format("Your type had been changed from {0} to {1}", oldType.ToString(),
						type.ToString()));
				}
			}
		}
	}
}