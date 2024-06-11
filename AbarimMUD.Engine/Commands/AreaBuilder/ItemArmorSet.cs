using AbarimMUD.Data;
using System.Text;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemArmorSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(3);
			if (parts.Length < 3)
			{
				if (parts.Length == 1 && parts[0].ToLower() == "type")
				{
					var sb = new StringBuilder();
					sb.Append("Usage: itemarmorset type _itemId_ ");
					sb.Append(CommandUtils.JoinForUsage<ArmorType>());

					context.Send(sb.ToString());
					return;
				}

				context.Send("Usage: itemarmorset type|armor _itemId_ _params_");
				return;
			}

			var item = context.EnsureItemType(parts[1], ItemType.Armor);
			if (item == null)
			{
				return;
			}

			var cmdText = parts[0].ToLower();
			var cmdData = parts[2];
			switch (cmdText)
			{
				case "type":
					{
						ArmorType type;
						if (!Enum.TryParse(cmdData, true, out type))
						{
							context.SendTextLine($"Unknown item type '{cmdData}'");
							return;
						}

						item.Value1 = (int)type;
						context.SendTextLine($"Changed {item.Id}'s type to '{type}'");
					}
					break;
				case "armor":
					{
						int val;
						if (!int.TryParse(cmdData, out val))
						{
							context.SendTextLine($"Unable to parse armor {cmdData}");
							return;
						}

						item.Value2 = val;
						context.SendTextLine($"Changed {item.Id}'s armor to '{val}'");
					}
					break;
				default:
					{
						context.Send(string.Format("Unknown item armor property '{0}'", cmdData));
						return;
					}
			}

			item.Save();
		}
	}
}
