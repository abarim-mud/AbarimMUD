using AbarimMUD.Commands;
using AbarimMUD.Data;

namespace AbarimMUD.Combat
{
	public static class CombatUtils
	{
		public static void Slain(this ExecutionContext attacker, ExecutionContext target)
		{
			target.Creature.Slain();

			var ripMessage = $"{target.Creature.ShortDescription} is dead! R.I.P.";
			attacker.Send(ripMessage);

			foreach (var roomContext in attacker.AllExceptMeInRoom())
			{
				roomContext.Send(ripMessage);
			}

			var character = attacker.Creature as Character;
			var targetMobile = target.Creature as MobileInstance;
			if (character != null && targetMobile != null)
			{
				var xpAward = targetMobile.Stats.XpAward;

				var lastLevel = character.Level;
				character.Experience += xpAward;
				attacker.Send($"Total exp for kill is {xpAward.FormatBigNumber()}.");

				// Append level up messages
				for (var level = lastLevel + 1; level <= character.Level; ++level)
				{
					var previousHp = character.Class.HitpointsRange.CalculateValue(level - 1);
					var newHp = character.Class.HitpointsRange.CalculateValue(level);
					attacker.Send($"Welcome to the level {level}! You gained {newHp - previousHp} hitpoints.");
				}

				character.Wealth += targetMobile.Info.Wealth;
				attacker.Send($"You get {targetMobile.Info.Wealth.FormatBigNumber()} from the corpse of {targetMobile.ShortDescription}.");
				attacker.Send($"You buried the corpse of {targetMobile.ShortDescription}.");

				var roomMessage = $"{attacker.ShortDescription} gets gold coins from the corpse of {targetMobile.ShortDescription}.";
				foreach (var roomContext in attacker.AllExceptMeInRoom())
				{
					roomContext.Send(roomMessage);
				}
			}

			attacker.FightsWith = null;
			target.FightsWith = null;
		}


		public static void SingleAttack(this ExecutionContext attacker, int attackIndex, ExecutionContext target)
		{
			var stats = attacker.Creature.Stats;
			var attacks = stats.Attacks;

			var targetStats = target.Stats;

			var attack = attacks[attackIndex];
			var damage = CombatCalc.CalculateDamage(attack, targetStats.Armor);

			target.Creature.State.Hitpoints -= damage.Damage;
			var room = attacker.CurrentRoom;
			foreach (var character in room.Characters)
			{
				string message;
				if (damage.Damage <= 0)
				{
					string attackName;
					if (attacker.Creature != character)
					{
						attackName = $"Your {attack.AttackType.GetAttackNoun()}";
					}
					else
					{
						attackName = $"{attacker.ShortDescription}'s {attack.AttackType.GetAttackNoun()}";
					}

					string targetName;
					if (target.Creature != character)
					{
						targetName = $"armor of {target.Creature.ShortDescription}";
					}
					else
					{
						targetName = "your armor";
					}

					message = $"{attackName} couldn't pierce through {targetName}.";
				}
				else
				{
					string attackerName;
					if (attacker.Creature != character)
					{
						attackerName = attacker.ShortDescription;
					}
					else
					{
						attackerName = "You";
					}

					string targetName;
					if (target.Creature != character)
					{
						targetName = target.Creature.ShortDescription;
					}
					else
					{
						targetName = "you";
					}

					message = GetAttackMessage(damage, attackerName, targetName, attack.AttackType);
				}

				var context = (PlayerExecutionContext)character.Tag;
				context.Send(message);
			}

			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.Slain(target);
			}
		}

		private static string GetAttackMessage(DamageResult damageResult, string attackerName, string targetName, AttackType attackType)
		{
			string result;
			string attackVerb, massacre, massacre2;

			if (attackerName == "You")
			{
				attackVerb = attackType.GetAttackNoun();
				massacre = "massacre";
				massacre2 = $"your {attackType.GetAttackNoun()}";
			}
			else
			{
				attackVerb = attackType.GetAttackVerb();
				massacre = "massacres";
				massacre2 = $"its {attackType.GetAttackNoun()}";
			}

			var damage = damageResult.Damage;
			if (damage < 5)
			{
				result = $"{attackerName} barely {attackVerb} {targetName} ({damageResult}).";
			}
			else if (damage < 10)
			{
				result = $"{attackerName} {attackVerb} {targetName} ({damageResult}).";
			}
			else if (damage < 15)
			{
				result = $"{attackerName} {attackVerb} {targetName} hard ({damageResult}).";
			}
			else if (damage < 20)
			{
				result = $"{attackerName} {attackVerb} {targetName} very hard ({damageResult}).";
			}
			else if (damage < 25)
			{
				result = $"{attackerName} {attackVerb} {targetName} extremelly hard ({damageResult}).";
			}
			else if (damage < 30)
			{
				result = $"{attackerName} {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}
			else if (damage < 50)
			{
				result = $"{attackerName} brutally {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}
			else
			{
				result = $"{attackerName} viciously {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}

			return result;
		}

		public static string GetEvadeMessage(string attackerName, string targetName, AttackType attackType)
		{
			return $"{targetName} evades {attackType.GetAttackNoun()} of {attackerName}.";
		}
	}
}
