﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Wear : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: wear <itemName>");
				return;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return;
			}

			var result = context.Creature.Wear(item.Item);
			if (result == true)
			{
				// Remove from inv
				context.Creature.Inventory.AddItem(item.Item, -1);
				context.Send($"You wear {item.ShortDescription}");
			}
			else if (result == false)
			{
				context.Send($"You can't wear {item.ShortDescription}, since that slot is occupied");
			}
			else
			{
				context.Send($"{item.ShortDescription} can't be worn");
			}

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();
		}
	}
}
