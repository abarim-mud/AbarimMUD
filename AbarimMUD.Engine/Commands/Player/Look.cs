﻿using System.Text;
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
				name += string.Format(" #{0}", room.Id);
			}

			name += $" ({room.SectorType})";

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

			if (room.Characters.Count > 0 || room.Mobiles.Count > 0)
			{
				sb.Append("[yellow]");
			}

			// Characters
			foreach (var character in room.Characters)
			{
				if (character == context.Creature)
				{
					continue;
				}

				sb.AppendLine($"{character.NameAndTitle()} is standing here.");
			}

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

				sb.AppendLine(desc.Trim());
			}

			if (room.Mobiles.Count > 0 || room.Characters.Count > 0)
			{
				sb.Append("[reset]");
			}


			return sb.ToString();
		}

		private string BuildCreatureHealthState(Creature creature)
		{
			var hpPercentage = creature.State.Hitpoints * 100 / creature.Stats.MaxHitpoints;

			if (hpPercentage >= 100)
			{
				return "is in excellent condition.";
			}
			else if (hpPercentage >= 90)
			{
				return "has a few scratches.";
			}
			else if (hpPercentage >= 75)
			{
				return "has some small wounds and bruises.";
			}
			else if (hpPercentage >= 50)
			{
				return "has quite a few wounds.";
			}
			else if (hpPercentage >= 30)
			{
				return "has some big nasty wounds and scratches.";
			}
			else if (hpPercentage >= 15)
			{
				return "looks pretty hurt.";
			}

			return "is in awful condition.";
		}

		private string BuildCreatureDescription(ExecutionContext context, ExecutionContext lookContext)
		{
			var creature = lookContext.Creature;

			var sb = new StringBuilder();

			if (!string.IsNullOrEmpty(creature.Description))
			{
				sb.AppendLine(creature.Description.TrimEnd());
			}

			sb.AppendLine($"{creature.ShortDescription} {BuildCreatureHealthState(creature)}");

			var grid = creature.BuildEquipmentDesc();
			if (grid != null)
			{
				sb.AppendLine();
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

				sb.AppendLine("Level: " + creature.Level);

				sb.AppendLine();

				var stats = creature.Stats;

				var regen = stats.GetHitpointsRegen(lookContext.IsFighting);
				sb.AppendLine($"Hitpoints: {creature.State.Hitpoints}/{stats.MaxHitpoints} + {regen}");
				sb.AppendLine("Armor Class: " + creature.Stats.Armor);
				for (var i = 0; i < stats.Attacks.Count; i++)
				{
					var attack = stats.Attacks[i];
					sb.AppendLine($"Attack #{i + 1}: {attack}");
				}

				if (mobile != null)
				{
					sb.AppendLine("Xp Award: " + stats.CalculateXpAward().FormatBigNumber());
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

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();


			var sb = new StringBuilder();

			do
			{
				if (string.IsNullOrEmpty(data))
				{
					// Look room
					sb.AppendLine(BuildRoomDescription(context, context.Room));
					break;
				}

				// Look for creature in room
				var lookContext = context.Room.Find(data);
				if (lookContext != null)
				{
					sb.AppendLine($"You look at {lookContext.ShortDescription}.");
					sb.AppendLine();
					sb.AppendLine(BuildCreatureDescription(context, lookContext));

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
					sb.AppendLine($"You look at {item.Name}.");
					sb.AppendLine();
					sb.AppendLine(BuildItemDescription(context, item.Item));
					break;
				}

				sb.AppendLine(string.Format("There isnt '{0}' in this room", data));
			} while (false);

			context.Send(sb.ToString());

			return true;
		}
	}
}
