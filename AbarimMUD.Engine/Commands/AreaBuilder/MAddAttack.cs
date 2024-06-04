using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	internal class MAddAttack : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			int id, accuracy, minDamage, maxDamage;
			string typeStr;
			if (!data.Parse(out id, out typeStr, out accuracy, out minDamage, out maxDamage))
			{
				context.Send("Usage: maddattack _mobileId_ _type_ _accuracy_ _minDamage_ _maxDamage_");
				return;
			}

			AttackType attackType;
			if (!typeStr.TryParseEnumUncased(out attackType))
			{
				context.Send($"Unable to parse attack type {typeStr}");
				return;
			}

			if (minDamage < 0)
			{
				context.Send($"minDamage couldn't be negative");
				return;
			}

			if (maxDamage < 0)
			{
				context.Send($"maxDamage couldn't be negative");
				return;
			}

			if (minDamage > maxDamage)
			{
				context.Send($"minDamage couldn't be bigger than maxDamage");
				return;
			}

			var mobile = Area.GetMobileById(id);
			if (mobile == null)
			{
				context.Send($"Unable to find mobile info with id {id}");
				return;
			}

			var newAttack = new Attack
			{
				AttackType = attackType,
				Accuracy = accuracy,
				DamageRange = new RandomRange(minDamage, maxDamage)
			};

			mobile.Attacks.Add(newAttack);
			mobile.Area.Save();

			context.Send($"Added #{mobile.Attacks.Count - 1} attack ({newAttack}) to {mobile.ShortDescription} (#{mobile.Id})");
		}
	}
}
