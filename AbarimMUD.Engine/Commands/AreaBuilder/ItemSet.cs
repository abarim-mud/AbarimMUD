using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(3);
			if (parts.Length < 3)
			{
				if (parts.Length == 1 && parts[0].ToLower() == "type")
				{
					var sb = new StringBuilder();
					sb.Append("Usage: itemset type _itemId_ ");
					sb.Append(CommandUtils.JoinForUsage<ItemType>());
					context.Send(sb.ToString());
					return;
				}

				context.Send("Usage: itemset keywords|short|long|desc|type _itemId_ _params_");
				return;
			}

			var id = parts[1];
			var item = context.EnsureItemById(id);
			if (item == null)
			{
				return;
			}

			var cmdText = parts[0].ToLower();
			var cmdData = parts[2];

			switch (cmdText)
			{
				case "keywords":
					{
						item.Keywords = cmdData.SplitByWhitespace().ToHashSet();
						context.Send($"Changed {item.Id}'s keywords to '{item.Keywords.JoinKeywords()}'");
					}
					break;
				case "short":
					{
						item.ShortDescription = cmdData;
						context.Send($"Changed {item.Id}'s short to '{item.ShortDescription}'");
					}
					break;
				case "long":
					{
						item.LongDescription = cmdData;
						context.Send($"Changed {item.Id}'s long to '{item.LongDescription}'");
					}
					break;
				case "desc":
					{
						item.Description = cmdData;
						context.Send($"Changed {item.Id}'s desc to '{item.Description}'");
					}
					break;
				case "type":
					{
						ItemType type;
						if (!Enum.TryParse(cmdData, true, out type))
						{
							context.Send($"Unknown item type '{cmdData}'");
							return;
						}

						item.ItemType = type;
						context.Send($"Changed {item.Id}'s type to '{type}'");
					}
					break;
				default:
					{
						context.Send(string.Format("Unknown item property '{0}'", cmdData));
						return;
					}
			}

			item.Save();
		}
	}
}