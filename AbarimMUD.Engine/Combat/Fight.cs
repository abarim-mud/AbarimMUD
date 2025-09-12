using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AbarimMUD.Combat
{
	public enum FightSide
	{
		Side1,
		Side2
	}

	public class Fight
	{
		private static readonly List<Fight> _allFights = new List<Fight>();

		private DateTime _lastRound;
		private List<ExecutionContext> _participants = new List<ExecutionContext>();

		public Room Room { get; private set; }

		public bool Finished { get; private set; }

		public IReadOnlyCollection<ExecutionContext> Participants => _participants;

		public static IReadOnlyList<Fight> AllFights => _allFights;

		private Fight(ExecutionContext attacker, ExecutionContext target)
		{
			_lastRound = DateTime.Now;

			Room = attacker.Room;

			Add(FightSide.Side1, attacker, target);
		}

		public void Add(FightSide fightSide, ExecutionContext attacker, ExecutionContext target)
		{
			// Set FightsInfos
			if (attacker.FightInfo.Fight == null)
			{
				attacker.FightInfo.Fight = this;
				attacker.FightInfo.Side = fightSide;
				attacker.FightInfo.Target = target;
			}

			if (target.FightInfo.Fight == null)
			{
				target.FightInfo.Fight = this;
				target.FightInfo.Side = fightSide.GetOppositeSide();
				target.FightInfo.Target = attacker;
			}

			// Update participants
			if (!_participants.Contains(attacker))
			{
				_participants.Add(attacker);
			}

			if (!_participants.Contains(target))
			{
				_participants.Add(target);
			}
		}

		public void LeaveFight(ExecutionContext participant)
		{
			if (!_participants.Contains(participant))
			{
				return;
			}

			// Null targets
			foreach (var p in _participants)
			{
				if (p == participant)
				{
					continue;
				}

				if (p.FightInfo.Target == participant)
				{
					p.FightInfo.Target = null;
				}
			}

			participant.FightInfo.Fight = null;
			participant.FightInfo.Target = null;
			_participants.Remove(participant);

			ValidateFight();
		}

		private void ValidateFight()
		{
			var side1Count = 0;
			var side2Count = 0;

			foreach (var p in _participants)
			{
				if (p.FightInfo.Side == FightSide.Side1)
				{
					++side1Count;
				}
				else
				{
					++side2Count;
				}
			}

			if (side1Count == 0 || side2Count == 0)
			{
				End();
			}
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

			foreach (var creature in _participants)
			{
				for (var i = 0; i < creature.Stats.Attacks.Count; ++i)
				{
					if (creature.FightInfo.Target == null)
					{
						// Choose next target
						var targetSide = creature.FightInfo.Side.GetOppositeSide();
						var target = (from p in _participants where p.FightInfo.Side == targetSide select p).FirstOrDefault();

						if (target == null)
						{
							Debug.Assert(false);

							// This should neven happen
							// Still end fight in such case
							End();
							goto finish;
						}

						creature.FightInfo.Target = target;
					}

					creature.SingleAttack(i, creature.FightInfo.Target);

					// If the target dies, it should trigger removal of dead participant and the fight validation
					// Which may result in the fight finish
					if (Finished)
					{
						goto finish;
					}
				}
			}

		finish:;
		}

		public void End()
		{
			foreach (var p in _participants)
			{
				p.FightInfo.Fight = null;
				p.FightInfo.Target = null;
			}

			_participants.Clear();
			Finished = true;
		}

		public static void Start(ExecutionContext attacker, ExecutionContext target)
		{
			attacker.BreakHunt();
			if (!attacker.IsAlive || !target.IsAlive || attacker == target || attacker.FightInfo.Fight != null)
			{
				return;
			}

			if (target.FightInfo.Fight != null)
			{
				// Add to existing fight
				target.FightInfo.Fight.Add(target.FightInfo.Side.GetOppositeSide(), attacker, target);
			}
			else
			{
				// Create new fight
				var fight = new Fight(attacker, target);
				_allFights.Add(fight);
			}
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

	public static class FightExtensions
	{
		public static FightSide GetOppositeSide(this FightSide side)
		{
			return side == FightSide.Side1 ? FightSide.Side2 : FightSide.Side1;
		}
	}
}
