using AbarimMUD.Data;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder
{
	public class ItemCreate : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: itemcreate _newItemId");
				return;
			}

			var id = data;
			var existing = Item.GetItemById(id);
			if (existing != null)
			{
				context.Send($"Id {id} is used by {existing} already");
				return;
			}

			// Create new item
			var newItem = new Item
			{
				Id = id,
				Keywords = new HashSet<string> { "unset" },
				ShortDescription = "Unset",
				Description = "Unset"
			};

			newItem.Create();

			context.Send($"New item '{newItem}' was created");

			ItemSpawn.Execute(context, newItem.Id.ToString());
		}
	}
}