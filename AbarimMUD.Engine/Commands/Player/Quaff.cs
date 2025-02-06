using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Quaff: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: quaff <itemName>");
				return false;
			}

			var item = context.EnsureItemInInventory(data);
			if (item == null)
			{
				return false;
			}

			if (item.Info.ItemType != ItemType.Potion)
			{
				context.Send("Only potions could be quaffed.");
				return false;
			}

			context.Creature.Inventory.AddItem(item.Item, -1);
			
			foreach(var pair in item.Info.Affects)
			{
				var affect = pair.Value;
				context.Creature.AddTemporaryAffect(affect.AffectSlotName, item.ShortDescription, affect);
			}

			context.Send($"You quaff '{item.ShortDescription}'.");
			context.SendRoomExceptMe($"{context.Creature.ShortDescription} quaffed {item.ShortDescription}.");

			var asCharacter = context.Creature as Character;
			asCharacter?.Save();


			return true;
		}
	}
}
