using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.SendTextLine("Usage: itemcreate _newItemId");
				return;
			}

			var id = data;
			var existing = Item.GetItemById(id);
			if (existing != null)
			{
				context.SendTextLine($"Id {id} is used by {existing} already");
				return;
			}

			// Create new item
			var newItem = new Item
			{
				Id = id,
				Name = "unset",
				ShortDescription = "Unset",
				Description = "Unset"
			};

			newItem.Create();

			context.SendTextLine($"New item '{newItem}' was created");

			ItemSpawn.Execute(context, newItem.Id.ToString());
		}
	}
}