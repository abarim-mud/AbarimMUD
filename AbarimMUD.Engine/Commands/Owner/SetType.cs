using System.Collections.Generic;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Owner
{
	public sealed class SetType : OwnerCommand
	{
		private static readonly Dictionary<string, Role> _namesToTypes = new Dictionary<string, Role>();

		static SetType()
		{
			_namesToTypes["player"] = Role.Player;
			_namesToTypes["builder"] = Role.Builder;
			_namesToTypes["admin"] = Role.Administrator;
			_namesToTypes["owner"] = Role.Owner;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 2)
			{
				context.Send("Usage: settype <character> player|area|world|lowadmin|highadmin|owner");
				return;
			}

			var characterName = parts[0];

			// Find the character
			var character = Character.GetCharacterByName(characterName);
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
				context.Send(string.Format("{0}'s type is {1} already.", character.Name, type.ToString()));
				return;
			}

			var oldType = character.Role;

			// Change
			character.Role = type;
			character.Save();

			context.Send(string.Format("Changed {0}'s type from {1} to {2}", character.Name, oldType.ToString(),
				type.ToString()));

			// Check if it's online
			foreach (var s in Server.Instance.Sessions)
			{
				if (s.Character != null && s.Character.Name == character.Name)
				{
					// It's online
					// Inform him or her that the type had been changed
					s.Context.Send(string.Format("Your type had been changed from {0} to {1}", oldType.ToString(),
						type.ToString()));
				}
			}
		}
	}
}