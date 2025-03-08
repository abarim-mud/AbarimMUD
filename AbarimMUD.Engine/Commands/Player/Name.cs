using System.Collections.Generic;

namespace AbarimMUD.Commands.Player
{
	public class Name : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Name who?");
				return false;
			}

			string name = null;
			HashSet<string> keywords = null;

			// Search among mobiles
			var targetMobile = context.Room.Find(data);
			if (targetMobile != null)
			{
				name = targetMobile.ShortDescription;
				keywords = targetMobile.Creature.Keywords;
			}

			// Search among inventory items
			if (keywords == null)
			{
				var targetItem = context.Creature.Inventory.FindItem(data);
				if (targetItem != null)
				{
					name = targetItem.Name;
					keywords = targetItem.Info.Keywords;
				}
			}

			if (keywords == null)
			{
				context.Send($"Couldn't find a mobile or an item that matches '{data}'.");
				return false;
			}

			var s = string.Join(" ", keywords);
			context.Send($"{name}'s keywords are '{s}'");
			return true;
		}
	}
}
