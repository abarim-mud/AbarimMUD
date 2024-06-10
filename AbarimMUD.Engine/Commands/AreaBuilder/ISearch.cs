using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ISearch : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var pat = data.Trim().ToLower();
			if (string.IsNullOrEmpty(pat))
			{
				context.SendTextLine("Usage: isearch _pattern_");
				return;
			}
			else
			{
				var found = false;
				foreach (var item in Item.Storage)
				{
					if (item.Name.Contains(pat))
					{
						found = true;
						context.SendTextLine(item.ToString());
					}
				}

				if (!found)
				{
					context.SendTextLine($"No items found which names contained '{pat}'");
				}
			}
		}
	}
}
