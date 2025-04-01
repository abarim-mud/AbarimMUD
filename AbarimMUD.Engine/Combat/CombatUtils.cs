using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Text;

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

		private static string FormatMessage(string message, ExecutionContext user, ExecutionContext target, string weapon, string info)
		{
			if (string.IsNullOrEmpty(message))
			{
				return message;
			}

			var result = new StringBuilder();
			var variable = new StringBuilder();

			var readingVariable = false;
			for (var i = 0; i < message.Length; ++i)
			{
				var c = message[i];

				if (c == '{' && !readingVariable)
				{
					variable.Clear();
					readingVariable = true;
				}
				else if (c == '}' && readingVariable)
				{
					var v = variable.ToString().ToLower();

					string value;
					switch (v)
					{
						case "user.name":
							value = user.ShortDescription;
							break;

						case "target.name":
							value = target.ShortDescription;
							break;

						case "weapon":
							value = weapon;
							break;

						case "info":
							value = info;
							break;

						default:
							throw new Exception($"Unknown variable '{v}'");
					}

					result.Append(value);
					readingVariable = false;
				}
				else
				{
					if (!readingVariable)
					{
						result.Append(c);
					}
					else
					{
						variable.Append(c);
					}
				}
			}

			if (readingVariable)
			{
				throw new Exception("Unfinished variable name");
			}

			return result.ToString();
		}

		private static string FormatDetails(string info)
		{
			if (string.IsNullOrEmpty(info))
			{
				return string.Empty;
			}
			return " (" + info + ")";
		}

		private static void SendMissMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = FormatDetails(info);
			attacker.SendBattleMessage(FormatMessage(ability.MessageMissUser, attacker, target, weapon, info));
			attacker.SendRoomExceptMe(FormatMessage(ability.MessageMissRoom, attacker, target, weapon, info));
		}

		private static void SendHitMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = FormatDetails(info);
			attacker.SendBattleMessage(FormatMessage(ability.MessageHitUser, attacker, target, weapon, info));
			attacker.SendRoomExceptMe(FormatMessage(ability.MessageHitRoom, attacker, target, weapon, info));
		}

		private static void SendKillMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = FormatDetails(info);
			attacker.SendBattleMessage(FormatMessage(ability.MessageKillUser, attacker, target, weapon, info));
			attacker.SendRoomExceptMe(FormatMessage(ability.MessageKillRoom, attacker, target, weapon, info));
		}

		private static FightDetails GetFightDetails(this ExecutionContext ctx)
		{
			var fightDetails = FightDetails.Damage;
			var character = ctx.Creature as Character;
			if (character != null)
			{
				fightDetails = character.FightDetails;
			}

			return fightDetails;
		}

		public static void SingleAttack(this ExecutionContext attacker, int attackIndex, ExecutionContext target)
		{
			var fightDetails = attacker.GetFightDetails();
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

					if (fightDetails == FightDetails.All)
					{
						message += FormatDetails(attackRoll.ToString());
					}

					message += ".";
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

					message = GetAttackMessage(attackRoll, damage, attackerName, targetName, attack.Type, fightDetails);
				}

				var context = (ExecutionContext)character.Tag;
				context.SendBattleMessage(message);
			}

			if (target.Creature.State.Hitpoints < 0)
			{
				attacker.Slain(target);
			}
		}

		private static void AbilityAttack(this ExecutionContext attacker,
			Ability ability, Func<int> damageRoller, int successChancePercentage,
			ItemInstance weapon, ExecutionContext target, out bool slain)
		{
			slain = false;

			var fightDetails = attacker.GetFightDetails();
			var weaponStr = weapon != null ? weapon.Info.ShortDescription : string.Empty;

			attacker.State.Moves -= ability.MovesCost;

			var skillRollSuccess = Utility.RollPercentage(successChancePercentage);
			var attackRoll = new AttackRollResult();
			if (skillRollSuccess)
			{
				// Then hit change
				var attack = attacker.Stats.Attacks[0];
				attackRoll = attack.DoAttackRoll(target.Stats.Armor);
			}

			var damage = 0;
			if (attackRoll.Hit)
			{
				// Now roll the damage
				damage = damageRoller();
			}

			var info = string.Empty;
			if (damage == 0)
			{
				// Miss
				if (fightDetails == FightDetails.All)
				{
					if (!skillRollSuccess)
					{
						info = "skill";
					}
					else if (!attackRoll.Hit)
					{
						info = attackRoll.ToString();
					}
					else
					{
						info = "no damage";
					}
				}

				attacker.SendMissMessage(ability, target, weaponStr, info);
				return;
			}

			switch (fightDetails)
			{
				case FightDetails.Damage:
					info = damage.ToString();
					break;

				case FightDetails.All:
					info = attackRoll.AttackRoll + ", " + damage;
					break;
			}


			target.Creature.State.Hitpoints -= damage;
			if (target.Creature.State.Hitpoints >= 0)
			{
				attacker.SendHitMessage(ability, target, weaponStr, info);
				return;
			}

			attacker.SendKillMessage(ability, target, weaponStr, info);
			attacker.Slain(target);
			slain = true;
		}

		public static void Backstab(this ExecutionContext attacker, ItemInstance weapon, ExecutionContext target)
		{
			// Success chance 95 - (i * 20)
			for (var i = 0; i < attacker.Stats.BackstabCount; ++i)
			{
				var successChancePercentage = 95 - (i * 20);
				bool slain;
				attacker.AbilityAttack(Ability.Backstab,
					() =>
					{
						var damage = 0;
						var attack = attacker.Stats.Attacks[0];
						for (var j = 0; j < attacker.Stats.BackstabMultiplier; ++j)
						{
							damage += attack.CalculateDamage();
						}

						return damage;
					},
					successChancePercentage, weapon, target, out slain);

				if (slain)
				{
					break;
				}
			}
		}

		public static void Circlestab(this ExecutionContext attacker, ItemInstance weapon, ExecutionContext target)
		{
			bool slain;
			attacker.AbilityAttack(Ability.Circlestab, () =>
				{
					var damage = 0;
					var circleMultiplier = attacker.Stats.BackstabMultiplier / 3;
					var attack = attacker.Stats.Attacks[0];
					for (var j = 0; j < circleMultiplier; ++j)
					{
						damage += attack.CalculateDamage();
					}

					return damage;
				},
				95, weapon, target, out slain);
		}

		public static void Kick(this ExecutionContext attacker, ExecutionContext target)
		{
			bool slain;
			attacker.AbilityAttack(Ability.Kick, () =>
				{
					var baseDamage = Math.Min(attacker.Level, 20);
					var damageRange = new ValueRange(baseDamage, baseDamage + 4);
					return damageRange.Random();
				},
				95, null, target, out slain);
		}

		public static void Deathtouch(this ExecutionContext attacker, ExecutionContext target)
		{
			bool slain;
			attacker.AbilityAttack(Ability.Deathtouch, () =>
				{
					var damage = 0;
					var attack = attacker.Stats.Attacks[0];
					for (var j = 0; j < attacker.Stats.DeathtouchMultiplier; ++j)
					{
						damage += attack.CalculateDamage();
					}

					return damage;
				},
				95, null, target, out slain);
		}

		private static string GetAttackMessage(AttackRollResult attackRoll, int damage, string attackerName, string targetName, AttackType attackType, FightDetails fightDetails)
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

			switch (fightDetails)
			{
				case FightDetails.None:
					break;
				case FightDetails.Damage:
					result += FormatDetails(damage.ToString());
					break;
				case FightDetails.All:
					result += FormatDetails($"{attackRoll}, {damage}");
					break;
			}

			result += ".";

			return result;
		}

		public static string GetEvadeMessage(string attackerName, string targetName, AttackType attackType)
		{
			return $"{targetName} evades {attackType.GetAttackNoun()} of {attackerName}.";
		}
	}
}
