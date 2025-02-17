using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class StabWeapon : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Only players can set the stabweapon.");
				return false;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: stabweapon _itemName_|off");
				return false;
			}

			if (data.EqualsToIgnoreCase("off"))
			{
				character.StabWeapon = string.Empty;
				character.Save();

				context.Send("Cleared the current stabweapon.");
				return true;
			}

			// Check the player has the skill
			var ab = context.Stats.GetAbility("backstab");
			if (ab == null || context.Creature.Stats.BackstabCount == 0)
			{
				context.Send($"You don't know how to backstab.");
				return false;
			}

			// Check if the parameter corresponds to the wielded weapon
			bool isWielded;
			var weapon = context.Creature.FindStabWeapon(data, out isWielded);
			if (weapon == null)
			{
				context.Send($"Couldn't find a stab weapon with keyword {data}.");
				return false;
			}

			character.StabWeapon = data;
			character.Save();

			var message = $"Stabweapon was set to '{data}'. It corresponds to an item '{weapon.Name}'";
			if (isWielded)
			{
				message += " (wielded)";
			}
			context.Send(message);

			return true;
		}
	}
}
