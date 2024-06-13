using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{

		public static Race EnsureRace(this ExecutionContext context, string name)
		{
			var race = Race.GetRaceById(name);
			if (race == null)
			{
				context.Send($"Unable to find race '{name}'");
			}

			return race;
		}

		public static GameClass EnsureClass(this ExecutionContext context, string name)
		{
			var cls = GameClass.GetClassById(name);
			if (cls == null)
			{
				context.Send($"Unable to find class '{name}'");
			}

			return cls;
		}

		public static bool EnsureInt(this ExecutionContext context, string value, out int result)
		{
			if (!int.TryParse(value, out result))
			{
				context.Send($"Unable to parse number '{value}'");
				return false;
			}

			return true;
		}

		public static bool EnsureEnum<T>(this ExecutionContext context, string value, out T result) where T : struct
		{
			if (!Enum.TryParse(value, true, out result))
			{
				context.Send($"Unable to parse enum '{value}'");
				return false;
			}

			return true;
		}


		public static InventoryRecord EnsureItemInInventory(this ExecutionContext context, string name)
		{
			var item = context.Creature.Inventory.FindItem(name);
			if (item == null)
			{
				context.Send($"Unable to find item '{name}'");
			}

			return item;
		}

		public static WearItem EnsureItemWorn(this ExecutionContext context, string name)
		{
			var item = context.Creature.Equipment.FindItem(name);
			if (item == null)
			{
				context.Send($"You arent wearing an item '{name}'");
			}

			return item;
		}

		public static string JoinForUsage<T>() where T : struct, Enum
		{
			return string.Join('|', from e in Enum.GetValues<T>() select e.ToString().ToLower());
		}

		private static T EnsureById<T>(this ExecutionContext context, string id, Func<string, T> getter)
		{
			var item = getter(id);
			if (item == null)
			{
				var name = typeof(T).Name.ToLower();
				context.Send($"Unable to find {name} with id '{id}'");
			}

			return item;

		}

		public static Item EnsureItemById(this ExecutionContext context, string id) => EnsureById(context, id, Item.GetItemById);
		public static Mobile EnsureMobileById(this ExecutionContext context, string id) => EnsureById(context, id, Mobile.GetMobileById);
		public static Character EnsureCharacterByName(this ExecutionContext context, string name) => EnsureById(context, name, Character.GetCharacterByName);

		public static Item EnsureItemType(this ExecutionContext context, string id, ItemType itemType)
		{
			var item = Item.EnsureItemById(id);
			if (item == null)
			{
				return null;
			}

			if (item.ItemType != itemType)
			{
				context.Send($"Item {item} isnt {itemType.ToString().ToLower()}");
				return null;
			}

			return item;
		}

		public static AsciiGrid BuildEquipmentDesc(this Creature creature)
		{
			AsciiGrid grid = null;

			var y = 0;
			foreach (var eq in creature.Equipment.Items)
			{
				if (eq.Item == null)
				{
					continue;
				}

				if (grid == null)
				{
					grid = new AsciiGrid();
				}

				grid.SetValue(0, y, $"<worn on {eq.Slot.ToString().ToLower()}>");
				grid.SetValue(1, y, eq.Item.ShortDescription);

				++y;
			}

			return grid;
		}
	}
}