using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Circlestab : PlayerCommand
	{
		public override bool CanFightskill => true;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ab = context.EnsurePhysicalByName("circlestab");
			if (ab == null)
			{
				return false;
			}

			// Check the player weapon can stab
			var weapon = context.Creature.Equipment.GetSlot(EquipmentSlotType.Wield).Item;
			if (weapon == null)
			{
				context.Send($"You can't circlestab with the bare hands.");
				return false;
			}

			if (!weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				context.Send($"You can't circlestab with this weapon.");
				return false;
			}

			return context.UseAbility(ab, data, () =>
			{
				var damage = 0;
				var attack = context.Stats.Attacks[0];
				for (var j = 0; j < ab.Power; ++j)
				{
					damage += attack.CalculateDamage();
				}

				return damage;
			}, weapon);
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "") => Ability.Circlestab.PhysicalCommandLagInMs;
	}
}
