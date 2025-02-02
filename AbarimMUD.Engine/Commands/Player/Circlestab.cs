﻿using AbarimMUD.Combat;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Circlestab : PlayerCommand
	{
		public override bool CanAutoskill => true;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ab = context.Creature.Stats.GetAbility(AbilityType.Physical, "circlestab");
			if (ab == null)
			{
				context.Send($"You don't know how to circlestab.");
				return false;
			}

			// Check the player weapon can stab
			var weapon = context.Creature.Equipment[SlotType.Wield];
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

			data = data.Trim();
			if (!context.IsFighting && string.IsNullOrEmpty(data))
			{
				context.Send($"Circlestab who?");
				return false;
			}

			ExecutionContext target = null;
			if (!string.IsNullOrWhiteSpace(data))
			{
				target = context.Room.Find(data);
				if (target == null)
				{
					context.Send($"There isnt '{data}' in this room");
					return false;
				}

				if (target == context)
				{
					context.Send("You can't circlestab yourself.");
					return false;
				}

				if (target.Creature is Character)
				{
					context.Send($"You can't attack {target.ShortDescription}");
					return false;
				}
			}

			if (context.State.Moves < CombatCalc.CirclestabMovesCost())
			{
				context.Send($"You're too tired to circlestab.");
				return false;
			}

			if (target == null)
			{
				target = context.FightInfo.Target;
			}

			context.Circlestab(weapon, target);
			Fight.Start(context, target);

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "")
		{
			return Configuration.PauseBetweenFightRoundsInMs * 2 / 3;
		}

		public override CommandCost CalculateCost(ExecutionContext context, string data = "")
		{
			return new CommandCost(0, 0, CombatCalc.CirclestabMovesCost());
		}
	}
}
