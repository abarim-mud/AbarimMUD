using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Deathtouch : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ab = context.Stats.GetAbilityById("deathtouch");
			if (ab == null)
			{
				context.Send($"You don't know how to deathtouch.");
				return false;
			}

			// Check the player weapon
			var weapon = context.Creature.Equipment.GetSlot(EquipmentSlotType.Wield).Item;
			if (weapon != null)
			{
				context.Send("You can't deathtouch when wielding a weapon.");
				return false;
			}

			return context.UseAbility(ab, data,
				() =>
				{
					var damage = 0;
					var attack = context.Stats.Attacks[0];
					for (var j = 0; j < ab.Power; ++j)
					{
						damage += attack.CalculateDamage();
					}

					return damage;
				}, null);
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "") => Ability.Deathtouch.PhysicalCommandLagInMs;
	}
}
