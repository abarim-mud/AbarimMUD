using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemWeaponSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 4)
			{
				context.Send("Usage: itemweaponset _itemId_ _penetration_ _minimumDamage_ _maximumDamage_");
				return;
			}

			var item = context.EnsureItemType(parts[0], ItemType.Weapon);
			if (item == null)
			{
				return;
			}

			int penetration;
			if (!context.EnsureInt(parts[1], out penetration))
			{
				return;
			}

			int minimumDamage;
			if (!context.EnsureInt(parts[2], out minimumDamage))
			{
				return;
			}

			int maximumDamage;
			if (!context.EnsureInt(parts[3], out maximumDamage))
			{
				return;
			}

			item.Value1 = penetration;
			item.Value2 = minimumDamage;
			item.Value3 = maximumDamage;
			context.Send($"Changed {item.Id}'s minimum-maximum damage to {minimumDamage}-{maximumDamage}");

			item.Save();

			Creature.InvalidateAllCreaturesStats();
		}
	}
}
	