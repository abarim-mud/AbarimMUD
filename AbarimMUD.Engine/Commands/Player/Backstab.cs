using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Backstab: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else.");
				return false;
			}

			// Check the player has the skill
			var ab = context.Stats.GetAbility("backstab");
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

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Backstab who?");
				return false;
			}

			var target = context.Room.Find(data);
			if (target == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return false;
			}

			if (target == context)
			{
				context.Send("You can't backstab yourself.");
				return false;
			}

			if (target.Creature is Character)
			{
				context.Send($"You can't attack {target.ShortDescription}");
				return false;
			}

			if (target.Creature.State.Hitpoints < target.Creature.Stats.MaxHitpoints)
			{
				context.Send($"You can't backstab a wounded creature.");
				return false;
			}

			if (context.State.Moves < ab.MovesCost)
			{
				context.Send($"You're too tired to backstab.");
				return false;
			}

			if (weapon != null && weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				// Wielded weapon can stab
				context.Backstab(ab, weapon, target);
			} else if (weapon == null)
			{
				// Wield stab and remove, since figthing with bare hands
				if (!context.WearItem(stabWeapon))
				{
					return false;
				}
				
				context.Backstab(ab, stabWeapon, target);
				
				context.RemoveItem(EquipmentSlotType.Wield);
			} else
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

				context.Backstab(ab, stabWeapon, target);
				if (context.RemoveItem(EquipmentSlotType.Wield))
				{
					context.WearItem(weapon);
				}
			}

			Fight.Start(context, target);

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "")
		{
			return Configuration.PauseBetweenFightRoundsInMs * 2 / 3;
		}

		public override CommandCost CalculateCost(ExecutionContext context, string data = "")
		{
			return new CommandCost(0, 0, Ability.Backstab.MovesCost);
		}
	}
}
