using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class ItemWeaponSet : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 3)
			{
				context.Send("Usage: itemweaponset _itemId_ _minimumDamage_ _maximumDamage_");
				return;
			}

			var item = context.EnsureItemType(parts[0], ItemType.Weapon);
			if (item == null)
			{
				return;
			}

			int minimumDamage;
			if (!context.EnsureInt(parts[1], out minimumDamage))
			{
				return;
			}

			int maximumDamage;
			if (!context.EnsureInt(parts[2], out maximumDamage))
			{
				return;
			}

			item.Value1 = minimumDamage;
			item.Value2 = maximumDamage;
			context.Send($"Changed {item.Id}'s minimum-maximum damage to {minimumDamage}-{maximumDamage}");

			item.Save();
		}
	}
}
