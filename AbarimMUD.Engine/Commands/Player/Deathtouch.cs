using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Deathtouch : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else.");
				return false;
			}

			// Check the player has the skill
			var ab = context.Stats.GetAbility("deathtouch");
			if (ab == null || context.Creature.Stats.DeathtouchMultiplier == 0)
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

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Deathtouch who?");
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
				context.Send("You can't deathtouch yourself.");
				return false;
			}

			if (target.Creature is Character)
			{
				context.Send($"You can't attack {target.ShortDescription}");
				return false;
			}

			if (target.Creature.State.Hitpoints < target.Creature.Stats.MaxHitpoints)
			{
				context.Send($"You can't deathtouch a wounded creature.");
				return false;
			}

			if (context.State.Moves < ab.MovesCost)
			{
				context.Send($"You're too tired to deathtouch.");
				return false;
			}

			context.Deathtouch(target);
			Fight.Start(context, target);

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "")
		{
			return Configuration.PauseBetweenFightRoundsInMs;
		}

		public override CommandCost CalculateCost(ExecutionContext context, string data = "")
		{
			return new CommandCost(0, 0, Ability.Deathtouch.MovesCost);
		}
	}
}
