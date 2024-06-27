using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Circlestab : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			if (context.Creature.GetSkill("circlestab") == null)
			{
				context.Send($"You don't know how to circlestab.");
				return;
			}

			// Check the player weapon can stab
			var weapon = context.Creature.Equipment[SlotType.Wield];
			if (weapon == null)
			{
				context.Send($"You can't circlestab with the bare hands.");
				return;
			}

			if (!weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				context.Send($"You can't circlestab with this weapon.");
				return;
			}

			data = data.Trim();
			if (!context.IsFighting && string.IsNullOrEmpty(data))
			{
				context.Send($"Circlestab who?");
				return;
			}

			ExecutionContext target = null;
			if (!string.IsNullOrWhiteSpace(data))
			{
				target = context.Room.Find(data);
				if (target == null)
				{
					context.Send($"There isnt '{data}' in this room");
					return;
				}

				if (target == context)
				{
					context.Send("You can't circlestab yourself.");
					return;
				}

				if (target.Creature is Character)
				{
					context.Send($"You can't attack {target.ShortDescription}");
					return;
				}
			}

			if (context.State.Moves < CombatCalc.CirclestabMovesCost())
			{
				context.Send($"You're too tired to circlestab.");
				return;
			}

			if (target == null)
			{
				target = context.FightInfo.Target;
			}

			context.Circlestab(weapon, target);
			Fight.Start(context, target);
		}
	}
}
