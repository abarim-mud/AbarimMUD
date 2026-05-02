using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;

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
				var xpAward = targetMobile.Info.CalculateXpAward();
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

			// Make sure target hp is negative
			// Which will force its death
			if (target.State.Hitpoints > 0)
			{
				target.State.Hitpoints = -1;
			}
		}

		public static string FormatDetails(string info)
		{
			if (string.IsNullOrEmpty(info))
			{
				return string.Empty;
			}

			return " (" + info + ")";
		}

		public static FightDetails GetFightDetails(this ExecutionContext ctx)
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

		public static int ApplyResistance(int damage, int resistance)
		{
			if (resistance == 0)
			{
				return damage;
			}


			if (resistance >= 100)
			{
				return 0;
			}

			var k = 1.0f - resistance / 100.0f;
			return (int)(damage * k);
		}
	}
}
