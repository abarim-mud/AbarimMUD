using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemSearch : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: itemsearch _pattern_");
				return;
			}
			else
			{
				var found = false;
				foreach (var item in Item.Storage)
				{
					if (item.MatchesKeyword(data))
					{
						found = true;
						context.Send(item.ToString());
					}
				}

				if (!found)
				{
					context.Send($"No items found which names contained '{data}'");
				}
			}
		}
	}
}
