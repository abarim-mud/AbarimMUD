using AbarimMUD.Combat;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Commands
{
	internal static class AbilityHelper
	{
		private static readonly int[] DefaultAbilityChecks = new int[] { Configuration.DefaultAbilityCheck };

		private const string DefaultMessageMissUser = "You miss {target.name} with {ability}{info}.";
		private const string DefaultMessageMissRoom = "{user.name} misses {target.name} with {ability}{info}.";
		private const string DefaultMessageHitUser = "You hit {target.name} with {ability}{info}.";
		private const string DefaultMessageHitRoom = "{user.name} hit {target.name} with {ability}{info}.";
		private const string DefaultMessageKillUser = "You kill {target.name} with {ability}{info}.";
		private const string DefaultMessageKillRoom = "{user.name} kills {target.name} with {ability}{info}.";

		private struct DamageInfo
		{
			public int PhysicalDamage { get; set; }
			public int MagicDamage { get; set; }
			public int HolyDamage { get; set; }
			public int FireDamage { get; set; }
			public int ColdDamage { get; set; }
			public int ShockDamage { get; set; }
			public int ChaosDamage { get; set; }

			public int TotalDamage => PhysicalDamage + MagicDamage + HolyDamage + FireDamage + ColdDamage + ShockDamage + ChaosDamage;
		}

		private static string FormatMessage(string message, ExecutionContext user, Ability ability, ExecutionContext target, string weapon, string info)
		{
			var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "user.name", user.ShortDescription },
				{ "user.pronoun1", user.Sex.GetPronoun1()  },
				{ "user.pronoun2", user.Sex.GetPronoun2() },
				{ "user.pronoun3", user.Sex.GetPronoun3() },
				{ "target.name", target.ShortDescription },
				{ "target.pronoun1", target.Sex.GetPronoun1() },
				{ "target.pronoun2", target.Sex.GetPronoun2() },
				{ "target.pronoun3", target.Sex.GetPronoun3() },
				{ "ability", ability.Name },
				{ "weapon", weapon },
				{ "info", info }
			};

			return message.FormatMessage(variables);
		}

		private static void SendMissMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = CombatUtils.FormatDetails(info);

			var message = DefaultMessageMissUser;
			if (!string.IsNullOrEmpty(ability.MessageMissUser))
			{
				message = ability.MessageMissUser;
			}
			attacker.SendBattleMessage(FormatMessage(message, attacker, ability, target, weapon, info));

			message = DefaultMessageMissRoom;
			if (!string.IsNullOrEmpty(ability.MessageMissRoom))
			{
				message = ability.MessageMissRoom;
			}
			attacker.SendRoomExceptMe(FormatMessage(message, attacker, ability, target, weapon, info));
		}

		private static void SendHitMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = CombatUtils.FormatDetails(info);

			var message = DefaultMessageHitUser;
			if (!string.IsNullOrEmpty(ability.MessageHitUser))
			{
				message = ability.MessageHitUser;
			}
			attacker.SendBattleMessage(FormatMessage(message, attacker, ability, target, weapon, info));

			message = DefaultMessageHitRoom;
			if (!string.IsNullOrEmpty(ability.MessageHitRoom))
			{
				message = ability.MessageHitRoom;
			}
			attacker.SendRoomExceptMe(FormatMessage(message, attacker, ability, target, weapon, info));
		}

		private static void SendKillMessage(this ExecutionContext attacker, Ability ability, ExecutionContext target, string weapon, string info)
		{
			info = CombatUtils.FormatDetails(info);

			var message = DefaultMessageKillUser;
			if (!string.IsNullOrEmpty(ability.MessageHitUser))
			{
				message = ability.MessageHitUser;
			}
			if (!string.IsNullOrEmpty(ability.MessageKillUser))
			{
				message = ability.MessageKillUser;
			}
			attacker.SendBattleMessage(FormatMessage(message, attacker, ability, target, weapon, info));

			message = DefaultMessageKillRoom;
			if (!string.IsNullOrEmpty(ability.MessageHitRoom))
			{
				message = ability.MessageHitRoom;
			}
			if (!string.IsNullOrEmpty(ability.MessageKillRoom))
			{
				message = ability.MessageKillRoom;
			}
			attacker.SendRoomExceptMe(FormatMessage(message, attacker, ability, target, weapon, info));
		}

		public static bool UseAbility(this ExecutionContext context, AbilityPower ap, string targetName, int[] abilityChecks, Func<int> physicalDamageRoller, ItemInstance weapon)
		{
			var ability = ap.Ability;
			if (ability.Flags.Contains(AbilityFlags.NotFighting) && context.IsFighting)
			{
				context.Send($"You're too busy fighting with someone else.");
				return false;
			}

			ExecutionContext target = null;
			if (!string.IsNullOrWhiteSpace(targetName))
			{
				target = context.Room.Find(targetName);
				if (target == null)
				{
					context.Send($"There isnt '{targetName}' in this room");
					return false;
				}

				if (ap.Ability.Flags.Contains(AbilityFlags.Offensive))
				{
					if (target == context)
					{
						context.Send($"You can't use '{ability.Name}' on yourself.");
						return false;
					}

					if (!context.CheckPk(target))
					{
						return false;
					}
				}
			}
			else
			{
				if (ap.Ability.Flags.Contains(AbilityFlags.Offensive))
				{
					if (!context.IsFighting)
					{
						context.Send($"{ability.Name} who?");
						return false;
					}

					target = context.FightInfo.Target;
				}
				else
				{
					target = context;
				}
			}

			if (ability.Flags.Contains(AbilityFlags.TargetFullHp) && target.Creature.State.Hitpoints < target.Creature.Stats.MaxHitpoints)
			{
				context.Send($"You can't {ability.Name} a wounded creature.");
				return false;
			}

			if (context.Creature.State.Moves < ability.MovesCost)
			{
				context.Send("Not enough moves.");
				return false;
			}

			if (context.Creature.State.Mana < ability.ManaCost)
			{
				context.Send("Not enough mana.");
				return false;
			}

			for (var i = 0; i < abilityChecks.Length; ++i)
			{
				context.Creature.State.Mana -= ability.ManaCost;
				context.Creature.State.Moves -= ability.MovesCost;

				// Ability check
				var fightDetails = context.GetFightDetails();
				var info = string.Empty;
				int rnd;

				var abilityCheck = abilityChecks[i];
				var abilityRollSuccess = Utility.RollPercentage(abilityCheck, out rnd);
				if (!abilityRollSuccess)
				{
					if (fightDetails == FightDetails.All)
					{
						info = $" ({rnd})";
					}

					if (ability.Type == AbilityType.Physical)
					{
						context.Send($"You failed to {ability.Name}{info}.");
					}
					else
					{
						context.Send($"You lost your concentration{info}.");
					}

					break;
				}

				var damage = new DamageInfo();
				var weaponStr = weapon != null ? weapon.Info.ShortDescription : string.Empty;
				AttackRollResult? physicalAttackRoll = null;
				if (ability.Flags.Contains(AbilityFlags.Offensive) && ability.Type == AbilityType.Physical)
				{
					// Hit check
					var attack = context.Stats.Attacks[0];
					var attackRoll = attack.DoAttackRoll(target.Stats.Armor);

					if (!attackRoll.Hit)
					{
						// Miss
						if (fightDetails == FightDetails.All)
						{
							info = attackRoll.ToString();
						}

						context.SendMissMessage(ability, target, weaponStr, info);
						break;
					}

					physicalAttackRoll = attackRoll;

					// Now roll the damage
					damage.PhysicalDamage = physicalDamageRoller();
				}

				// Apply affects
				if (ability.Affects != null)
				{
					foreach (var pair in ability.Affects)
					{
						var affect = pair.Value;
						target.Creature.AddTemporaryAffect(affect.AffectSlotName, ability.Name, affect.Type, affect.Value ?? ap.Power, affect.DurationInSeconds.Value, ability.MessageDeactivatedUser);
					}
				}

				if (ability.InstantEffects != null)
				{
					foreach (var instantEffect in ability.InstantEffects)
					{
						var power = instantEffect.Power.Random();
						switch (instantEffect.Type)
						{
							case InstantEffectType.Heal:
								target.Creature.Heal(power, 0, 0);
								break;

							case InstantEffectType.MagicDamage:
								damage.MagicDamage += power;
								break;
							
							case InstantEffectType.HolyDamage:
								damage.HolyDamage += power;
								break;
							
							case InstantEffectType.FireDamage:
								damage.FireDamage += power;
								break;
							
							case InstantEffectType.ColdDamage:
								damage.ColdDamage += power;
								break;
							
							case InstantEffectType.ShockDamage:
								damage.ShockDamage += power;
								break;
							
							case InstantEffectType.ChaosDamage:
								damage.ChaosDamage += power;
								break;
						}
					}
				}

				if (ability.Type == AbilityType.Spell)
				{
					context.Send($"You cast '{ability.Name}'.");
				}

				// Apply resistances
				damage.PhysicalDamage = CombatUtils.ApplyResistance(damage.PhysicalDamage, target.Stats.PhysicalResistance);
				damage.MagicDamage = CombatUtils.ApplyResistance(damage.MagicDamage, target.Stats.MagicResistance);

				// Elemental damages path through corresponding resistance than through the magic resistance
				damage.HolyDamage = CombatUtils.ApplyResistance(damage.HolyDamage, target.Stats.HolyResistance);
				damage.HolyDamage = CombatUtils.ApplyResistance(damage.HolyDamage, target.Stats.MagicResistance);

				damage.FireDamage = CombatUtils.ApplyResistance(damage.FireDamage, target.Stats.FireResistance);
				damage.FireDamage = CombatUtils.ApplyResistance(damage.FireDamage, target.Stats.MagicResistance);

				damage.ColdDamage = CombatUtils.ApplyResistance(damage.ColdDamage, target.Stats.ColdResistance);
				damage.ColdDamage = CombatUtils.ApplyResistance(damage.ColdDamage, target.Stats.MagicResistance);

				damage.ShockDamage = CombatUtils.ApplyResistance(damage.ShockDamage, target.Stats.ShockResistance);
				damage.ShockDamage = CombatUtils.ApplyResistance(damage.ShockDamage, target.Stats.MagicResistance);	


				target.Creature.State.Hitpoints -= damage.TotalDamage;

				info = string.Empty;
				switch (fightDetails)
				{
					case FightDetails.Damage:
						info = damage.TotalDamage.ToString();
						break;

					case FightDetails.All:
						if (physicalAttackRoll != null)
						{
							info = physicalAttackRoll.Value.AttackRoll + ", " + damage.TotalDamage;
						}
						else
						{
							info = damage.TotalDamage.ToString();
						}
						break;
				}

				if (target.Creature.State.Hitpoints >= 0)
				{
					context.SendHitMessage(ability, target, weaponStr, info);
				}
				else
				{
					context.SendKillMessage(ability, target, weaponStr, info);
					context.Slain(target);
				}
			}

			if (ability.Flags.Contains(AbilityFlags.Offensive))
			{
				Fight.Start(context, target);
			}

			return true;
		}

		public static bool UseAbility(this ExecutionContext context, AbilityPower ap, string targetName, int abilityCheck, Func<int> physicalDamageRoller, ItemInstance weapon) =>
			context.UseAbility(ap, targetName, new[] { abilityCheck }, physicalDamageRoller, weapon);

		public static bool UseAbility(this ExecutionContext context, AbilityPower ap, string targetName, Func<int> physicalDamageRoller, ItemInstance weapon) =>
			context.UseAbility(ap, targetName, DefaultAbilityChecks, physicalDamageRoller, weapon);
	}
}
