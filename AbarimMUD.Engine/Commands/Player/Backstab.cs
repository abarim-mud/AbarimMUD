using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Backstab: PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else.");
				return;
			}

			// Check the player has the skill
			if (context.Creature.Stats.BackstabCount == 0)
			{
				context.Send($"You don't know how to backstab.");
				return;
			}

			// Check the player weapon can stab
			var weapon = context.Creature.Equipment[Data.SlotType.Wield];
			if (weapon == null)
			{
				context.Send($"You can't stab with the bare hands.");
				return;
			}

			if (!weapon.Info.Flags.Contains(Data.ItemFlags.Stab))
			{
				context.Send($"You can't stab with this weapon.");
				return;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Backstab who?");
				return;
			}

			var target = context.Room.Find(data);
			if (target == null)
			{
				context.Send($"There isnt '{data}' in this room");
				return;
			}

			if (target == context)
			{
				context.Send("You can't backstab yourself.");
				return;
			}

			if (target.Creature is Character)
			{
				context.Send($"You can't attack {target.ShortDescription}");
				return;
			}

			if (target.Creature.State.Hitpoints < target.Creature.Stats.MaxHitpoints)
			{
				context.Send($"You can't backstab a wounded creature.");
				return;
			}

			if (context.State.Moves < CombatCalc.BackstabMovesCost())
			{
				context.Send($"You're too tired to sneak on anyone.");
				return;
			}

			context.Backstab(weapon, target);
			Fight.Start(context, target);
		}
	}
}
