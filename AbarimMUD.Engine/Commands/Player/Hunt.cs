﻿using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Hunt : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ability = context.EnsureAbility("hunt");
			if (ability == null)
			{
				return false;
			}

			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Hunt who?");
				return false;
			}

			// Search in the room
			var target = context.Room.Find(data);

			if (target == null)
			{
				// Search in the area
				target = context.CurrentArea.Find(data);
			}

			// Finally search in the world
			if (target == null)
			{
				foreach(var area in Area.Storage)
				{
					if (area.Name == context.CurrentArea.Name)
					{
						// Already searched
						continue;
					}

					target = area.Find(data);
					if (target != null)
					{
						break;
					}
				}
			}

			if (target == null)
			{
				context.Send($"Couldn't find anything named '{data}'.");
				return false;
			}

			if (target.Room.Id == context.Room.Id)
			{
				context.Send("It's right before you.");
				return false;
			}

			int moveSteps;
			var dir = PathFinding.FindFirstStep(context.Room, target.Room, out moveSteps);
			if (dir == null)
			{
				context.Send($"{target.ShortDescription} can't be reached.");
				return false;
			}

			context.Send($"You start to hunt {target.ShortDescription}");
			context.HuntTarget = target.Creature;
			context.LastHunt = DateTime.Now;

			return true;
		}
	}
}
