using AbarimMUD.Data;
using System.Text;

namespace AbarimMUD.Commands.Builder
{
	public class ItemWeaponSet : BuilderCommand
	{
		private enum PropertyType
		{
			AttackType,
			Penetration,
			Damage
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length == 0)
			{
				context.Send("Usage: itemweaponset attackType|penetration|damage _itemId_ _params_");
				return;
			}

			PropertyType setType;
			if (!context.EnsureEnum(parts[0], out setType))
			{
				return;
			}

			if (parts.Length < 3 || (setType == PropertyType.Damage && parts.Length < 4))
			{
				switch (setType)
				{
					case PropertyType.AttackType:
						var sb = new StringBuilder();
						sb.Append("Usage: itemweaponset attacktype _itemId_ ");
						sb.Append(CommandUtils.JoinForUsage<AttackType>());

						context.Send(sb.ToString());
						break;
					case PropertyType.Penetration:
						context.Send("Usage: itemweaponset penetration _itemId_ _penetration_");
						break;
					case PropertyType.Damage:
						context.Send("Usage: itemweaponset damage _itemId_ _minimum_ _maximum_");
						break;
				}

				return;
			}

			var item = context.EnsureItemType(parts[1], ItemType.Weapon);
			if (item == null)
			{
				return;
			}

			switch (setType)
			{
				case PropertyType.AttackType:
					if (!context.EnsureEnum<AttackType>(parts[2], out var attackType))
					{
						return;
					}

					item.Value1 = (int)attackType;
					context.Send($"Changed {item.Id}'s attackType to {attackType}");
					break;
				case PropertyType.Penetration:
					int penetration;
					if (!context.EnsureInt(parts[2], out penetration))
					{
						return;
					}

					item.Value2 = penetration;
					context.Send($"Changed {item.Id}'s penetration to {penetration}");
					break;
				case PropertyType.Damage:
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

					item.Value3 = minimumDamage;
					item.Value4 = maximumDamage;
					context.Send($"Changed {item.Id}'s minimum-maximum damage to {minimumDamage}-{maximumDamage}");
					break;
			}

			item.Save();

			Creature.InvalidateAllCreaturesStats();
		}
	}
}
