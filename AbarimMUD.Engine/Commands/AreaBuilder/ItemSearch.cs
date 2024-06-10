using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemSearch : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.SendTextLine("Usage: itemsearch _pattern_");
				return;
			}
			else
			{
				var found = false;
				foreach (var pair in Area.Storage.AllItems)
				{
					var item = pair.Value;
					if (item.Name.Contains(data))
					{
						found = true;
						context.SendTextLine(item.ToString());
					}
				}

				if (!found)
				{
					context.SendTextLine($"No items found which names contained '{data}'");
				}
			}
		}
	}
}
