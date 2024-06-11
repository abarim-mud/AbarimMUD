using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{

		public static Race EnsureRace(this ExecutionContext context, string name)
		{
			var race = Race.GetRaceById(name);
			if (race == null)
			{
				context.SendTextLine($"Unable to find race '{name}'");
			}

			return race;
		}

		public static GameClass EnsureClass(this ExecutionContext context, string name)
		{
			var cls = GameClass.GetClassById(name);
			if (cls == null)
			{
				context.SendTextLine($"Unable to find class '{name}'");
			}

			return cls;
		}

		public static bool EnsureInt(this ExecutionContext context, string value, out int result)
		{
			if (!int.TryParse(value, out result))
			{
				context.SendTextLine($"Unable to parse number {result}");
				return false;
			}

			return true;
		}

		public static InventoryRecord EnsureItemInInventory(this ExecutionContext context, string name)
		{
			var item = context.Creature.Inventory.FindItem(name);
			if (item == null)
			{
				context.SendTextLine($"Unable to find item '{name}'");
			}

			return item;
		}
	}
}