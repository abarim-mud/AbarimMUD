using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Combat
{
	public static class CombatUtils
	{
		public static void Slain(this ExecutionContext attacker, ExecutionContext target)
		{
			var ripMessage = $"{target.Creature.ShortDescription} is dead! R.I.P.";
			attacker.SendBattleMessage(ripMessage);

			foreach (var roomContext in attacker.AllExceptMeInRoom())
			{
				roomContext.SendBattleMessage(ripMessage);
			}

			var character = attacker.Creature as Character;
			var targetMobile = target.Creature as MobileInstance;
			if (character != null && targetMobile != null)
			{
				var xpAward = targetMobile.Stats.XpAward;
				attacker.Send($"Total exp for kill is {xpAward.FormatBigNumber()}.");

				attacker.AwardXp(xpAward);

				character.Gold += targetMobile.Info.Wealth;
				attacker.Send($"You get {targetMobile.Info.Wealth.FormatBigNumber()} gold coins from the corpse of {targetMobile.ShortDescription}.");
				attacker.Send($"You bury the corpse of {targetMobile.ShortDescription}.");

				var roomMessage = $"{attacker.ShortDescription} gets gold coins from the corpse of {targetMobile.ShortDescription}.\n" +
					$"{attacker.ShortDescription} buries the corpse of {targetMobile.ShortDescription}.";
				foreach (var roomContext in attacker.AllExceptMeInRoom())
				{
					roomContext.Send(roomMessage);
				}
			}
		}


		public static void SingleAttack(this ExecutionContext attacker, int attackIndex, ExecutionContext target)
		{
			var stats = attacker.Creature.Stats;
			var attacks = stats.Attacks;

			var targetStats = target.Stats;

			var attack = attacks[attackIndex];
			var damage = CombatCalc.CalculateDamage(attack, targetStats.Armor);

			target.Creature.State.Hitpoints -= damage.Damage;
			var room = attacker.Room;
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

				var context = (ExecutionContext)character.Tag;
				context.SendBattleMessage(message);
			}

			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.Slain(target);
			}
		}

		public static void Backstab(this ExecutionContext attacker, Ability ability, ItemInstance weapon, ExecutionContext target)
		{
			attacker.State.Moves -= ability.MovesCost;

			// Success chance 95 - (i * 20)
			for (var i = 0; i < attacker.Stats.BackstabCount; ++i)
			{
				var successChancePercentage = 95 - (i * 20);
				attacker.SendInfoMessage($"Backstab success chance: {successChancePercentage}%");
				var success = Utility.RollPercentage(successChancePercentage);
				if (!success)
				{
					attacker.SendBattleMessage($"{target.ShortDescription} quickly avoids your backstab and you nearly cut your own finger!");

					var roomMessage = $"{target.ShortDescription} quickly avoids {attacker.ShortDescription}'s backstab and {attacker.ShortDescription} nearly cuts their own finger!";
					foreach(var ch in attacker.AllExceptMeInRoom())
					{
						ch.SendBattleMessage(roomMessage);
					}

					return;
				}

				var damage = new DamageResult();
				var attack = attacker.Stats.Attacks[0];
				for (var j = 0; j < attacker.Stats.BackstabMultiplier; ++j)
				{
					var damage2 = CombatCalc.CalculateDamage(attack, target.Stats.Armor);

					damage += damage2;
				}

				target.Creature.State.Hitpoints -= damage.Damage;
				if (target.Creature.State.Hitpoints < 0)
				{
					attacker.SendBattleMessage($"{target.ShortDescription} makes a strange sound but is suddenly very silent as you place {weapon.Info.ShortDescription} in its back ({damage}).");

					var roomMessage = $"{target.ShortDescription} makes a strange sound but is suddenly very silent as {attacker.ShortDescription} places {weapon.Info.ShortDescription} in its back ({damage}).";
					foreach(var ch in  attacker.AllExceptMeInRoom())
					{
						ch.SendBattleMessage(roomMessage);
					}

					attacker.Slain(target);
					return;
				}

				if (damage.Damage <= 0)
				{
					attacker.SendBattleMessage($"Your blade scratches the armor of {target.ShortDescription} with the grinding sound!");

					var roomMessage = $"{attacker.ShortDescription}'s blade scratches the armor of {target.ShortDescription} with the grinding sound!";
					foreach (var ch in attacker.AllExceptMeInRoom())
					{
						ch.SendBattleMessage(roomMessage);
					}
				} else
				{
					attacker.SendBattleMessage($"You place {weapon.ShortDescription} in the back of {target.ShortDescription}, resulting in some strange noises and some blood ({damage})!");

					var roomMessage = $"{attacker.ShortDescription} places {weapon.ShortDescription} in the back of {target.ShortDescription}, resulting in some strange noises and some blood ({damage})!";
					foreach (var ch in attacker.AllExceptMeInRoom())
					{
						ch.Send(roomMessage);
					}
				}
			}
		}

		public static void Circlestab(this ExecutionContext attacker, Ability circlestab, ItemInstance weapon, ExecutionContext target)
		{
			attacker.State.Moves -= circlestab.MovesCost;

			// Success chance is 95%
			var successChancePercentage = 95;
			attacker.SendInfoMessage($"Circlestab success chance: {successChancePercentage}%");
			var success = Utility.RollPercentage(successChancePercentage);
			if (!success)
			{
				attacker.SendBattleMessage($"{target.ShortDescription} quickly avoids your circlestab and you nearly cut your own finger!");

				var roomMessage = $"{target.ShortDescription} quickly avoids {attacker.ShortDescription}'s circlestab and {attacker.ShortDescription} nearly cuts their own finger!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}

				return;
			}

			var damage = new DamageResult();

			var circleMultiplier = attacker.Stats.BackstabMultiplier / 3;
			var attack = attacker.Stats.Attacks[0];
			for (var j = 0; j < circleMultiplier; ++j)
			{
				var damage2 = CombatCalc.CalculateDamage(attack, target.Stats.Armor);

				damage += damage2;
			}

			target.Creature.State.Hitpoints -= damage.Damage;
			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.SendBattleMessage($"You struck {target.ShortDescription} right in the heart!");

				var roomMessage = $"{attacker.ShortDescription} strucks {target.ShortDescription} right in the heart!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}

				attacker.Slain(target);
				return;
			}

			if (damage.Damage <= 0)
			{
				attacker.SendBattleMessage($"Your blade scratches the armor of {target.ShortDescription} with the grinding sound!");

				var roomMessage = $"{attacker.ShortDescription}'s blade scratches the armor of {target.ShortDescription} with the grinding sound!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}
			}
			else
			{
				attacker.SendBattleMessage($"You quickly move from {target.ShortDescription}'s eyesight and stab it with {weapon.ShortDescription} ({damage})!");

				var roomMessage = $"{attacker.ShortDescription} quickly moves from {target.ShortDescription}'s eyesight and stabs it with {weapon.ShortDescription} ({damage})!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.Send(roomMessage);
				}
			}
		}

		public static void Kick(this ExecutionContext attacker, ExecutionContext target)
		{
			// Success chance is 95%
			var successChancePercentage = 95;
			attacker.SendInfoMessage($"Kick success chance: {successChancePercentage}%");
			var success = Utility.RollPercentage(successChancePercentage);
			if (!success)
			{
				attacker.SendBattleMessage($"You miss {target.ShortDescription} with your kick!");

				var roomMessage = $"{attacker.ShortDescription} misses {target.ShortDescription} with your kick!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}

				return;
			}


			var kickDamage = Math.Max(1, Math.Min(attacker.Level, 40) / 2);

			var attack = attacker.Stats.Attacks[0];
			var damage = CombatCalc.CalculateDamage(attack.Penetration, new RandomRange(kickDamage, kickDamage + 4), target.Stats.Armor);

			target.Creature.State.Hitpoints -= damage.Damage;
			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.SendBattleMessage($"Your masterful kick put the end of the life of {target.ShortDescription}!");

				var roomMessage = $"{attacker.ShortDescription}'s masterful kick puts the end of the life of {target.ShortDescription}!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}

				attacker.Slain(target);
				return;
			}

			if (damage.Damage <= 0)
			{
				attacker.SendBattleMessage($"Your kick can't break through the armor of {target.ShortDescription}!");

				var roomMessage = $"{attacker.ShortDescription}'s kick can't break through the armor of {target.ShortDescription}!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.SendBattleMessage(roomMessage);
				}
			}
			else
			{
				attacker.SendBattleMessage($"You kick {target.ShortDescription} ({damage})!");

				var roomMessage = $"{attacker.ShortDescription} kicks {target.ShortDescription} ({damage})!";
				foreach (var ch in attacker.AllExceptMeInRoom())
				{
					ch.Send(roomMessage);
				}
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
