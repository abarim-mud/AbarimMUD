using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Combat
{
	public class Fight
	{
		private static readonly List<Fight> _allFights = new List<Fight>();

		private DateTime _lastRound;

		public Room Room { get; private set; }
		public List<ExecutionContext> Side1 = new List<ExecutionContext>();
		public List<ExecutionContext> Side2 = new List<ExecutionContext>();
		public bool Finished => Side1.Count == 0 || Side2.Count == 0;

		public static IReadOnlyList<Fight> AllFights => _allFights;

		private Fight(ExecutionContext attacker, ExecutionContext target)
		{
			Side1.Add(attacker ?? throw new ArgumentNullException(nameof(attacker)));
			Side2.Add(target ?? throw new ArgumentNullException(nameof(target)));

			attacker.FightsWith = target;
			target.FightsWith = attacker;

			_lastRound = DateTime.Now;

			Room = attacker.CurrentRoom;
		}

		private void ProcessSide(List<ExecutionContext> side, List<ExecutionContext> otherSide)
		{
			foreach (var creature in side)
			{
				var stats = creature.Stats;
				for (var i = 0; i < creature.Stats.Attacks.Count; ++i)
				{
					var target = creature.FightsWith;
					creature.SingleAttack(i, target);

					if (!target.Creature.IsAlive)
					{
						// Target is dead
						otherSide.Remove(target);

						if (otherSide.Count == 0)
						{
							// No more targets
							goto finish;
						}

						// Choose next target
						creature.FightsWith = otherSide[0];
					}
				}
			}

		finish:;
		}

		public void DoRound()
		{
			if (Finished)
			{
				return;
			}

			var now = DateTime.Now;
			if ((now - _lastRound).TotalMilliseconds >= Configuration.PauseBetweenFightRoundsInMs)
			{
				_lastRound = now;
			}
			else
			{
				// Not enough time passed between rounds
				return;
			}

			ProcessSide(Side1, Side2);

			if (Finished)
			{
				return;
			}

			ProcessSide(Side2, Side1);
		}

		public void End()
		{
			// Nullify FightsWith
			foreach (var creature in Side1)
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

		public static void Start(ExecutionContext attacker, ExecutionContext target)
		{
			if (!attacker.IsAlive || !target.IsAlive)
			{
				return;
			}

			var fight = new Fight(attacker, target);
			_allFights.Add(fight);
		}

		public static void Process()
		{
			// Process fights
			foreach (var fight in AllFights)
			{
				fight.DoRound();
			}

			// Remove finished fights
			_allFights.RemoveAll(f => f.Finished);
		}
	}
}
