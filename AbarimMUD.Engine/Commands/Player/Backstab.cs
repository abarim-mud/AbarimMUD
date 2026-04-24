using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Backstab : PlayerCommand
	{
		private static bool DoBackstab(ExecutionContext attacker, AbilityPower backstab, string targetName, ItemInstance weapon)
		{
			if (attacker.Stats.BackstabCount < 1)
			{
				return true;
			}

			var abilityChecks = new int[attacker.Stats.BackstabCount];
			for (var i = 0; i < attacker.Stats.BackstabCount; ++i)
			{
				abilityChecks[i] = 95 - (i * 20);
			}

			return attacker.UseAbility(backstab, targetName, abilityChecks,
					() =>
					{
						var damage = 0;
						var attack = attacker.Stats.Attacks[0];
						for (var j = 0; j < backstab.Power; ++j)
						{
							damage += attack.CalculateDamage();
						}

						return damage;
					},
					weapon);
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ab = context.Stats.GetAbilityById("backstab");
			if (ab == null || context.Creature.Stats.BackstabCount == 0)
			{
				context.Send($"You don't know how to backstab.");
				return false;
			}

			// Check the player weapon can stab
			var character = context.Creature as Character;
			var weapon = context.Creature.Equipment.GetSlot(EquipmentSlotType.Wield).Item;
			ItemInstance stabWeapon = null;
			if (weapon == null || !weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				var baseMessage = weapon == null ?
					"You can't stab with the bare hands." :
					"You can't stab with this weapon.";

				if (character == null || string.IsNullOrEmpty(character.StabWeapon))
				{
					context.Send(baseMessage);
					return false;
				}

				bool isWielded;
				stabWeapon = context.Creature.FindStabWeapon(character.StabWeapon, out isWielded);
				if (stabWeapon == null)
				{
					context.Send(baseMessage);
					context.Send($"StabWeapon '{character.StabWeapon}' doesn't correspond to any item");
					return false;
				}
			}

			if (weapon != null && weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				// Wielded weapon can stab
				if (!DoBackstab(context, ab, data, weapon))
				{
					return false;
				}
			}
			else if (weapon == null)
			{
				// Wield stab and remove, since figthing with bare hands
				if (!context.WearItem(stabWeapon))
				{
					return false;
				}

				if (!DoBackstab(context, ab, data, stabWeapon))
				{
					return false;
				}

				context.RemoveItem(EquipmentSlotType.Wield);
			}
			else
			{
				// Remove non-stab weapon, wield stab, stab, remove stab, wield non-stab
				if (!context.RemoveItem(EquipmentSlotType.Wield))
				{
					return false;
				}

				if (!context.WearItem(stabWeapon))
				{
					// If we can't wield stab weapon, wield non-stab back and return false
					context.WearItem(weapon);
					return false;
				}

				if (!DoBackstab(context, ab, data, stabWeapon))
				{
					return false;
				}

				if (context.RemoveItem(EquipmentSlotType.Wield))
				{
					context.WearItem(weapon);
				}
			}

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "") => Ability.Backstab.PhysicalCommandLagInMs;
	}
}
