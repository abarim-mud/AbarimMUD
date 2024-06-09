namespace AbarimMUD.Commands.Player
{
	public class Kill : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.SendTextLine($"Kill who?");
				return;
			}

			var lookContext = context.CurrentRoom.Find(data);
			if (lookContext == null)
			{
				context.SendTextLine($"There isnt {data} in this room");
				return;
			}

			var asMobileContext = lookContext as MobileExecutionContext;
			if (asMobileContext == null)
			{
				context.SendTextLine($"You can't attack {data}");
				return;
			}

			// Attackers attacks first
			var attacks = context.Attacks;
			for(var i = 0; i < attacks.Length; ++i)
			{
				var attack = attacks[i];
				var damage = Combat.CalculateDamage(attack, asMobileContext.ArmorClass);

				if (damage.Damage <= 0)
				{
					context.SendTextLine($"Your {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {asMobileContext.Name}");

					foreach(var ctx in context.AllExceptMeInRoom())
					{
						ctx.SendTextLine($"{context.Name}'s {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {asMobileContext.Name}");
					}
				} else
				{
					context.SendTextLine(Strings.GetAttackMessage(damage, "You", asMobileContext.Name, attack.AttackType));

					foreach (var ctx in context.AllExceptMeInRoom())
					{
						ctx.SendTextLine(Strings.GetAttackMessage(damage, context.Name, asMobileContext.Name, attack.AttackType));
					}
				}
			}

			// Defenders attacks
			attacks = asMobileContext.Attacks;
			for (var i = 0; i < attacks.Length; ++i)
			{
				var attack = attacks[i];
				var damage = Combat.CalculateDamage(attack, context.ArmorClass);

				if (damage.Damage <= 0)
				{
					context.SendTextLine($"{asMobileContext.Name} couldn't pierce through your armor with {attack.AttackType.GetAttackNoun()}");

					foreach (var ctx in context.AllExceptMeInRoom())
					{
						ctx.SendTextLine($"{asMobileContext.Name}'s {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {context.Name}");
					}
				}
				else
				{
					context.SendTextLine(Strings.GetAttackMessage(damage, asMobileContext.Name, "you", attack.AttackType));

					foreach (var ctx in context.AllExceptMeInRoom())
					{
						ctx.SendTextLine(Strings.GetAttackMessage(damage, asMobileContext.Name, context.Name, attack.AttackType));
					}
				}
			}
		}
	}
}
