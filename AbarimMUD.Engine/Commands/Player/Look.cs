using System.Text;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public sealed class Look : PlayerCommand
	{
		private string BuildRoomDescription(ExecutionContext context, Room room)
		{
			var sb = new StringBuilder();
			sb.Append("[cyan]");

			var name = room.Name;
			if (context.IsStaff)
			{
				name += string.Format(" (#{0})", room.Id);
			}

			sb.AppendLine(name);
			sb.Append("[reset]");
			sb.Append("   ");
			sb.AppendLine(room.Description);
			sb.Append("[cyan]");
			sb.Append("Exits: ");

			var first = true;
			foreach (var pair in room.Exits)
			{
				var exit = pair.Value;
				if (!first)
				{
					sb.Append("[reset]");
					sb.Append(" ");
				}

				sb.Append("[cyan][underline]");
				sb.Append(exit.Direction.GetName());

				if (context.IsStaff)
				{
					sb.Append(string.Format("(#{0})", exit.TargetRoom.Id));
				}

				first = false;
			}

			sb.AppendLine("[reset]");

			// Mobiles
			foreach (var mobile in room.Mobiles)
			{
				var desc = mobile.Info.LongDescription;
				if (string.IsNullOrEmpty(desc))
				{
					desc = Defaults.DefaultMobileLongDesc(mobile.Info.ShortDescription);
				}

				if (context.IsStaff)
				{
					desc += string.Format(" (#{0})", mobile.Info.Id);
				}

				sb.AppendLine(desc);
			}

			// Characters
			foreach (var character in room.Characters)
			{
				var asPlayer = context as PlayerExecutionContext;
				if (asPlayer != null && asPlayer.Session.Character == character)
				{
					continue;
				}

				sb.AppendLine(string.Format("{0} is standing here.", character.Name));
			}

			return sb.ToString();
		}

		private string BuildCreatureDescription(ExecutionContext context, Creature creature)
		{
			var sb = new StringBuilder();

			if (!string.IsNullOrEmpty(creature.Description))
			{
				sb.AppendLine(creature.Description);
			}
			
			var grid = creature.BuildEquipmentDesc();
			if (grid != null)
			{
				sb.AppendLine($"{creature.ShortDescription} is using:");
				sb.Append(grid.ToString());
			}

			if (context.IsStaff)
			{
				sb.AppendLine();
				sb.Append("[cyan]");

				var mobile = creature as MobileInstance;
				if (mobile != null)
				{
					sb.AppendLine("Mobile Id: " + mobile.Info.Id);
				}

				sb.AppendLine("Class: " + creature.Class.Name);
				sb.AppendLine("Level: " + creature.Level);

				sb.AppendLine();

				var stats = creature.Stats;
				sb.AppendLine($"Hitpoints: {creature.State.Hitpoints}/{stats.MaxHitpoints}");
				sb.AppendLine("Armor Class: " + creature.Stats.Armor);
				for (var i = 0; i < stats.Attacks.Count; i++)
				{
					var attack = stats.Attacks[i];
					sb.AppendLine($"Attack #{i + 1}: {attack}");
				}

				sb.Append("[reset]");
			}

			return sb.ToString();
		}

		private string BuildItemDescription(ExecutionContext context, ItemInstance item)
		{
			var sb = new StringBuilder();

			sb.AppendLine(item.Info.Description);
			if (context.IsStaff)
			{
				sb.Append("[cyan]");
				sb.AppendLine("Item Id: " + item.Info.Id);
				sb.Append("[reset]");
			}

			return sb.ToString();
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();


			var sb = new StringBuilder();

			do
			{
				if (string.IsNullOrEmpty(data))
				{
					// Look room
					sb.AppendLine(BuildRoomDescription(context, context.CurrentRoom));
					break;
				}

				// Look for creature in room
				var lookContext = context.CurrentRoom.Find(data);
				if (lookContext != null)
				{
					sb.AppendLine($"You look at {lookContext.ShortDescription}.");
					sb.AppendLine();
					sb.AppendLine(BuildCreatureDescription(context, lookContext.Creature));

					if (lookContext != context)
					{
						lookContext.Send(string.Format("{0} looks at you.", context.ShortDescription));
					}

					foreach (var t in context.AllExceptMeInRoom())
					{
						if (t != lookContext)
						{
							t.Send(string.Format("{0} looks at {1}.", context.ShortDescription, lookContext.ShortDescription));
						}
					}

					break;
				}

				// Look for an item in inv
				var item = context.Creature.Inventory.FindItem(data);
				if (item != null)
				{
					sb.AppendLine($"You look at {item.ShortDescription}.");
					sb.AppendLine();
					sb.AppendLine(BuildItemDescription(context, item.Item));
					break;
				}

				sb.AppendLine(string.Format("There isnt '{0}' in this room", data));
			} while (false);

			context.Send(sb.ToString());
		}
	}
}
