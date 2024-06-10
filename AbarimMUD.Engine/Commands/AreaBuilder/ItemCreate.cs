using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Create new item
			var newItem = new Item
			{
				Id = Area.NextItemId,
				Name = "unset",
				ShortDescription = "Unset",
				LongDescription = "An item with 'unset' name is lying here.",
				Description = "Unset"
			};

			var area = context.CurrentRoom.Area;
			area.Items.Add(newItem);
			area.Save();

			context.SendTextLine($"New item '{newItem}' had been created for the area '{area.Name}'");

			ItemSpawn.Execute(context, newItem.Id.ToString());
		}
	}
}
