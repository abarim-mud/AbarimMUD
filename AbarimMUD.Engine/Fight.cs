using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AbarimMUD
{
	public class Fight
	{
		private class DamageLogRecord
		{
			public AttackType AttackType { get; }
			public Creature Attacker { get; }
			public Creature Target { get; }
			public DamageResult Damage { get; }

			public DamageLogRecord(AttackType attackType, Creature attacker, Creature target, DamageResult damage)
			{
				AttackType = attackType;
				Attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
				Target = target ?? throw new ArgumentNullException(nameof(target));
				Damage = damage;
			}
		}

		private readonly List<DamageLogRecord> _roundLog = new List<DamageLogRecord>();
		private DateTime? _lastRound;

		public Room Room { get; private set; }
		public List<Creature> Side1 = new List<Creature>();
		public List<Creature> Side2 = new List<Creature>();
		public bool Finished => Side1.Count == 0 || Side2.Count == 0;

		public Fight(Room room, Creature attacker, Creature target)
		{
			Room = room ?? throw new ArgumentNullException(nameof(room));
			Side1.Add(attacker ?? throw new ArgumentNullException(nameof(attacker)));
			Side2.Add(target ?? throw new ArgumentNullException(nameof(target)));

			attacker.FightsWith = target;
			target.FightsWith = attacker;
		}

		private void ProcessSide(List<Creature> side)
		{
			foreach (var creature in side)
			{
				var stats = creature.Stats;
				var targetStats = creature.FightsWith.Stats;
				var attacks = stats.Attacks;
				for (var i = 0; i < attacks.Count; ++i)
				{
					var attack = attacks[i];
					var damage = Combat.CalculateDamage(attack, targetStats.Armor);

					creature.FightsWith.State.Hitpoints -= damage.Damage;

					_roundLog.Add(new DamageLogRecord(attack.AttackType, creature, creature.FightsWith, damage));
				}
			}
		}

		public void DoRound()
		{
			var now = DateTime.Now;
			if (_lastRound == null)
			{
				_lastRound = now;
			} else if ((now - _lastRound.Value).TotalMilliseconds >= Configuration.PauseBetweenFightRoundsInMs)
			{
				_lastRound = now;
			} else
			{
				// Not enough time passed between rounds
				return;
			}

			_roundLog.Clear();

			ProcessSide(Side1);
			ProcessSide(Side2);

			foreach (var character in Room.Characters)
			{
				var sb = new StringBuilder();

				foreach (var record in _roundLog)
				{
					if (record.Damage.Damage <= 0)
					{
						string attackName;
						if (record.Attacker != character)
						{
							attackName = $"Your {record.AttackType.GetAttackNoun()}";
						}
						else
						{
							attackName = $"{record.Attacker.ShortDescription}'s {record.AttackType.GetAttackNoun()}";
						}

						string targetName;
						if (record.Target != character)
						{
							targetName = $"armor of {record.Target.ShortDescription}";
						}
						else
						{
							targetName = "your armor";
						}

						sb.AppendLine($"{attackName} couldn't pierce through {targetName}.");
					}
					else
					{
						string attackerName;
						if (record.Attacker != character)
						{
							attackerName = record.Attacker.ShortDescription;
						}
						else
						{
							attackerName = "You";
						}

						string targetName;
						if (record.Target != character)
						{
							targetName = record.Target.ShortDescription;
						}
						else
						{
							targetName = "you";
						}

						sb.AppendLine(GetAttackMessage(record.Damage, attackerName, targetName, record.AttackType));
					}
				}

				var context = (PlayerExecutionContext)character.Tag;
				context.Send(sb.ToString());
			}
		}

		public void End()
		{
			// Nullify FightsWith
			foreach(var creature in Side1)
			{
				creature.FightsWith = null;
			}

			foreach (var creature in Side2)
			{
				creature.FightsWith = null;
			}

			// Clear both sides
			Side1.Clear();
			Side2.Clear();
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
