using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Builder
{
	public class ItemSpawn : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var pat = data.Trim().ToLower();
			if (string.IsNullOrEmpty(pat))
			{
				context.Send("Usage: itemspawn _id_");
				return;
			}
			else
			{
				var item = Item.GetItemById(pat);
				if (item == null)
				{
					context.Send($"Unable to find an item with id {pat}");
				}
				else
				{
					context.Creature.Inventory.AddItem(new ItemInstance(item), 1);
					context.Send($"{item} appeared in your inventory");

					var asPlayer = context.Creature as Character;
					asPlayer?.Save();
				}
			}
		}
	}
}
