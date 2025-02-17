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
			attacker.SendRoomExceptMe(ripMessage);

			var character = attacker.Creature as Character;
			var targetMobile = target.Creature as MobileInstance;
			if (character != null && targetMobile != null)
			{
				var xpAward = targetMobile.Stats.CalculateXpAward();
				attacker.Send($"Total exp for kill is {xpAward.FormatBigNumber()}.");

				// Awarding Xp will do the save
				attacker.AwardXp(xpAward);

				var gold = targetMobile.Gold;
				if (gold > 0)
				{
					character.Gold += gold;
					attacker.Send($"You get {gold.FormatBigNumber()} gold coins from the corpse of {targetMobile.ShortDescription}.");
				}

				foreach (var item in targetMobile.Inventory.Items)
				{
					attacker.Creature.Inventory.AddItem(item);
					attacker.Send($"You get {item} from the corpse of {targetMobile.ShortDescription}.");
				}

				attacker.Send($"You bury the corpse of {targetMobile.ShortDescription}.");

				var roomMessage = $"{attacker.ShortDescription} gets gold coins from the corpse of {targetMobile.ShortDescription}.\n" +
					$"{attacker.ShortDescription} buries the corpse of {targetMobile.ShortDescription}.";
				attacker.SendRoomExceptMe(roomMessage);
			}
		}


		public static void SingleAttack(this ExecutionContext attacker, int attackIndex, ExecutionContext target)
		{
			var stats = attacker.Creature.Stats;
			var attacks = stats.Attacks;

			var attack = attacks[attackIndex];

			var targetStats = target.Stats;

			var damage = 0;

			var attackRoll = attack.DoAttackRoll(targetStats.Armor);

			if (attackRoll.Hit)
			{
				damage = attack.CalculateDamage();
			}

			target.Creature.State.Hitpoints -= damage;
			var room = attacker.Room;
			foreach (var character in room.Characters)
			{
				string message;
				if (damage <= 0)
				{
					if (attacker.Creature != character)
					{
						message = $"{attacker.ShortDescription} misses {target.Creature.ShortDescription} with its {attack.Type.GetAttackNoun()}";
					}
					else
					{
						message = $"You miss {target.Creature.ShortDescription} with your {attack.Type.GetAttackNoun()}";
					}

					message += $" ({attackRoll}).";
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

					message = GetAttackMessage(attackRoll, damage, attackerName, targetName, attack.Type);
				}

				var context = (ExecutionContext)character.Tag;
				context.SendBattleMessage(message);
			}

			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.Slain(target);
			}
		}

		private static bool RollSpecialAttack(this ExecutionContext attacker, ExecutionContext target, string attackName, int successChancePercentage)
		{
			var attack = attacker.Stats.Attacks[0];

			attacker.SendInfoMessage($"{attackName} success chance: {successChancePercentage}%");
			var success = Utility.RollPercentage(successChancePercentage);
			if (success)
			{
				// Then hit change
				var attackRoll = attack.DoAttackRoll(target.Stats.Armor);
				if (!attackRoll.Hit)
				{
					success = false;
				}

				attacker.SendInfoMessage($"Attack Roll: {attackRoll}");
			}

			return success;
		}


		public static void Backstab(this ExecutionContext attacker, Ability ability, ItemInstance weapon, ExecutionContext target)
		{
			attacker.State.Moves -= ability.MovesCost;

			// Success chance 95 - (i * 20)
			for (var i = 0; i < attacker.Stats.BackstabCount; ++i)
			{
				// Firstly roll overall success chance
				var successChancePercentage = 95 - (i * 20);
				var success = RollSpecialAttack(attacker, target, "Backstab", successChancePercentage);
				if (!success)
				{
					attacker.SendBattleMessage($"{target.ShortDescription} quickly avoids your backstab and you nearly cut your own finger!");

					var roomMessage = $"{target.ShortDescription} quickly avoids {attacker.ShortDescription}'s backstab and {attacker.ShortDescription} nearly cuts their own finger!";
					attacker.SendRoomExceptMe(roomMessage);

					return;
				}

				// Now roll the damage
				var damage = 0;
				var attack = attacker.Stats.Attacks[0];
				for (var j = 0; j < attacker.Stats.BackstabMultiplier; ++j)
				{
					damage += attack.CalculateDamage();
				}

				target.Creature.State.Hitpoints -= damage;
				if (target.Creature.State.Hitpoints < 0)
				{
					attacker.SendBattleMessage($"{target.ShortDescription} makes a strange sound but is suddenly very silent as you place {weapon.Name} in its back ({damage}).");

					var roomMessage = $"{target.ShortDescription} makes a strange sound but is suddenly very silent as {attacker.ShortDescription} places {weapon.Name} in its back ({damage}).";
					attacker.SendRoomExceptMe(roomMessage);

					attacker.Slain(target);
					return;
				}

				if (damage <= 0)
				{
					attacker.SendBattleMessage($"Your blade scratches the armor of {target.ShortDescription} with the grinding sound!");

					var roomMessage = $"{attacker.ShortDescription}'s blade scratches the armor of {target.ShortDescription} with the grinding sound!";
					attacker.SendRoomExceptMe(roomMessage);
				}
				else
				{
					attacker.SendBattleMessage($"You place {weapon.Name} in the back of {target.ShortDescription}, resulting in some strange noises and some blood ({damage})!");

					var roomMessage = $"{attacker.ShortDescription} places {weapon.Name} in the back of {target.ShortDescription}, resulting in some strange noises and some blood ({damage})!";
					attacker.SendRoomExceptMe(roomMessage);
				}
			}
		}

		public static void Circlestab(this ExecutionContext attacker, Ability circlestab, ItemInstance weapon, ExecutionContext target)
		{
			attacker.State.Moves -= circlestab.MovesCost;

			// Success chance is 95%
			var success = RollSpecialAttack(attacker, target, "Circlestab", 95);
			if (!success)
			{
				attacker.SendBattleMessage($"{target.ShortDescription} quickly avoids your circlestab and you nearly cut your own finger!");

				var roomMessage = $"{target.ShortDescription} quickly avoids {attacker.ShortDescription}'s circlestab and {attacker.ShortDescription} nearly cuts their own finger!";
				attacker.SendRoomExceptMe(roomMessage);

				return;
			}

			var damage = 0;
			var circleMultiplier = attacker.Stats.BackstabMultiplier / 3;
			var attack = attacker.Stats.Attacks[0];
			for (var j = 0; j < circleMultiplier; ++j)
			{
				damage += attack.CalculateDamage();
			}

			target.Creature.State.Hitpoints -= damage;
			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.SendBattleMessage($"You struck {target.ShortDescription} right in the heart!");

				var roomMessage = $"{attacker.ShortDescription} strucks {target.ShortDescription} right in the heart!";
				attacker.SendRoomExceptMe(roomMessage);

				attacker.Slain(target);
				return;
			}

			if (damage <= 0)
			{
				attacker.SendBattleMessage($"Your blade scratches the armor of {target.ShortDescription} with the grinding sound!");

				var roomMessage = $"{attacker.ShortDescription}'s blade scratches the armor of {target.ShortDescription} with the grinding sound!";
				attacker.SendRoomExceptMe(roomMessage);
			}
			else
			{
				attacker.SendBattleMessage($"You quickly move from {target.ShortDescription}'s eyesight and stab it with {weapon.Name} ({damage})!");

				var roomMessage = $"{attacker.ShortDescription} quickly moves from {target.ShortDescription}'s eyesight and stabs it with {weapon.Name} ({damage})!";
				attacker.SendRoomExceptMe(roomMessage);
			}
		}

		public static void Kick(this ExecutionContext attacker, ExecutionContext target)
		{
			// Success chance is 95%
			var success = RollSpecialAttack(attacker, target, "Kick", 95);
			if (!success)
			{
				attacker.SendBattleMessage($"You miss {target.ShortDescription} with your kick!");

				var roomMessage = $"{attacker.ShortDescription} misses {target.ShortDescription} with your kick!";
				attacker.SendRoomExceptMe(roomMessage);

				return;
			}

			var baseDamage = Math.Min(attacker.Level, 20);
			var damageRange = new ValueRange(baseDamage, baseDamage + 4);

			var damage = damageRange.Random();
			target.Creature.State.Hitpoints -= damage;
			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.SendBattleMessage($"Your masterful kick put the end of the life of {target.ShortDescription}!");

				var roomMessage = $"{attacker.ShortDescription}'s masterful kick puts the end of the life of {target.ShortDescription}!";
				attacker.SendRoomExceptMe(roomMessage);

				attacker.Slain(target);
				return;
			}

			if (damage <= 0)
			{
				attacker.SendBattleMessage($"Your kick can't break through the armor of {target.ShortDescription}!");

				var roomMessage = $"{attacker.ShortDescription}'s kick can't break through the armor of {target.ShortDescription}!";
				attacker.SendRoomExceptMe(roomMessage);
			}
			else
			{
				attacker.SendBattleMessage($"You kick {target.ShortDescription} ({damage})!");

				var roomMessage = $"{attacker.ShortDescription} kicks {target.ShortDescription} ({damage})!";
				attacker.SendRoomExceptMe(roomMessage);
			}
		}

		private static string GetAttackMessage(AttackRollResult attackRoll, int damage, string attackerName, string targetName, AttackType attackType)
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

			if (damage < 5)
			{
				result = $"{attackerName} barely {attackVerb} {targetName}";
			}
			else if (damage < 10)
			{
				result = $"{attackerName} {attackVerb} {targetName}";
			}
			else if (damage < 15)
			{
				result = $"{attackerName} {attackVerb} {targetName} hard";
			}
			else if (damage < 20)
			{
				result = $"{attackerName} {attackVerb} {targetName} very hard";
			}
			else if (damage < 25)
			{
				result = $"{attackerName} {attackVerb} {targetName} extremelly hard";
			}
			else if (damage < 30)
			{
				result = $"{attackerName} {massacre} {targetName} to small fragments with {massacre2}";
			}
			else if (damage < 50)
			{
				result = $"{attackerName} brutally {massacre} {targetName} to small fragments with {massacre2}";
			}
			else
			{
				result = $"{attackerName} viciously {massacre} {targetName} to small fragments with {massacre2}";
			}

			result += $" ({attackRoll}, {damage}).";

			return result;
		}

		public static string GetEvadeMessage(string attackerName, string targetName, AttackType attackType)
		{
			return $"{targetName} evades {attackType.GetAttackNoun()} of {attackerName}.";
		}
	}
}
