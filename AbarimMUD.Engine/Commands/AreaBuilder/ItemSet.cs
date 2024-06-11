using AbarimMUD.Data;
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
					sb.Append("Usage: itemset type ");
					sb.Append(string.Join('|', (from e in Enum.GetValues<ItemType>() select e.ToString().ToLower())));
					sb.Append(" _itemId_ _params_");
					context.Send(sb.ToString());
					return;
				}

				context.Send("Usage: itemset name|short|long|desc|type|val1|val2|val3|val4 _itemId_ _params_");
				return;
			}

			var id = parts[1];
			var item = Item.GetItemById(id);
			if (item == null)
			{
				context.Send(string.Format("Unable to find item with id {0}", id));
				return;
			}

			var cmdText = parts[0].ToLower();
			var cmdData = parts[2];

			switch (cmdText)
			{
				case "name":
					{
						item.Name = cmdData;
						context.SendTextLine($"Changed {item.Id}'s name to '{item.Name}'");
					}
					break;
				case "short":
					{
						item.ShortDescription = cmdData;
						context.SendTextLine($"Changed {item.Id}'s short to '{item.ShortDescription}'");
					}
					break;
				case "long":
					{
						item.LongDescription = cmdData;
						context.SendTextLine($"Changed {item.Id}'s long to '{item.LongDescription}'");
					}
					break;
				case "desc":
					{
						item.Description = cmdData;
						context.SendTextLine($"Changed {item.Id}'s desc to '{item.Description}'");
					}
					break;
				case "type":
					{
						ItemType type;
						if (!Enum.TryParse(cmdData, true, out type))
						{
							context.SendTextLine($"Unknown item type '{cmdData}'");
							return;
						}

						item.ItemType = type;
						context.SendTextLine($"Changed {item.Id}'s type to '{type}'");
					}
					break;
				case "val1":
					{
						int val;
						if (!context.EnsureInt(cmdData, out val))
						{
							return;
						}
						item.Value1 = val;
						context.SendTextLine($"Changed {item.Id}'s value1 to '{val}'");
					}
					break;
				case "val2":
					{
						int val;
						if (!context.EnsureInt(cmdData, out val))
						{
							return;
						}
						item.Value2 = val;
						context.SendTextLine($"Changed {item.Id}'s value2 to '{val}'");
					}
					break;
				case "val3":
					{
						int val;
						if (!context.EnsureInt(cmdData, out val))
						{
							return;
						}
						item.Value3 = val;
						context.SendTextLine($"Changed {item.Id}'s value3 to '{val}'");
					}
					break;
				case "val4":
					{
						int val;
						if (!context.EnsureInt(cmdData, out val))
						{
							return;
						}
						item.Value4 = val;
						context.SendTextLine($"Changed {item.Id}'s value1 to '{val}'");
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