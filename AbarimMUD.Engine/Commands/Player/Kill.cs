using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Kill : PlayerCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send($"Kill who?");
				return;
			}

			var lookContext = context.CurrentRoom.Find(data);
			if (lookContext == null)
			{
				context.Send($"There isnt {data} in this room");
				return;
			}

			var asMobileContext = lookContext as MobileExecutionContext;
			if (asMobileContext == null)
			{
				context.Send($"You can't attack {data}");
				return;
			}

			// Attackers attacks first
			var attacks = context.Attacks;

			var playerMessage = new StringBuilder();
			var roomMessage = new StringBuilder();
			for (var i = 0; i < attacks.Length; ++i)
			{
				var attack = attacks[i];
				var damage = Combat.CalculateDamage(attack, asMobileContext.ArmorClass);

				if (damage.Damage <= 0)
				{
					playerMessage.AppendLine($"Your {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {asMobileContext.ShortDescription}.");
					roomMessage.AppendLine($"{context.ShortDescription}'s {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {asMobileContext.ShortDescription}.");
				}
				else
				{
					playerMessage.AppendLine(Strings.GetAttackMessage(damage, "You", asMobileContext.ShortDescription, attack.AttackType));
					roomMessage.AppendLine(Strings.GetAttackMessage(damage, context.ShortDescription, asMobileContext.ShortDescription, attack.AttackType));
				}
			}

			playerMessage.AppendLine();
			roomMessage.AppendLine();

			// Defenders attacks
			attacks = asMobileContext.Attacks;
			for (var i = 0; i < attacks.Length; ++i)
			{
				var attack = attacks[i];
				var damage = Combat.CalculateDamage(attack, context.ArmorClass);

				if (damage.Damage <= 0)
				{
					playerMessage.AppendLine($"{asMobileContext.ShortDescription} couldn't pierce through your armor with {attack.AttackType.GetAttackNoun()}.");
					roomMessage.AppendLine($"{asMobileContext.ShortDescription}'s {attack.AttackType.GetAttackNoun()} couldn't pierce through armor of {context.ShortDescription}.");
				}
				else
				{
					playerMessage.AppendLine(Strings.GetAttackMessage(damage, asMobileContext.ShortDescription, "you", attack.AttackType));
					roomMessage.AppendLine(Strings.GetAttackMessage(damage, asMobileContext.ShortDescription, context.ShortDescription, attack.AttackType));
				}
			}

			context.Send(playerMessage.ToString());
			foreach (var ctx in context.AllExceptMeInRoom())
			{
				ctx.Send(roomMessage.ToString());
			}
		}
	}
}
