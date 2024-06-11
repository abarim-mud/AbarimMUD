using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemSpawn : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var pat = data.Trim().ToLower();
			if (string.IsNullOrEmpty(pat))
			{
				context.SendTextLine("Usage: itemspawn _id_");
				return;
			}
			else
			{
				var item = Item.GetItemById(pat);
				if (item == null)
				{
					context.SendTextLine($"Unable to find an item with id {pat}");
				}
				else
				{
					context.Creature.Inventory.AddItem(new ItemInstance(item) { Quantity = 1 });
					context.SendTextLine($"{item} appeared in your inventory");

					var asPlayer = context.Creature as Character;
					asPlayer?.Save();
				}
			}
		}
	}
}
