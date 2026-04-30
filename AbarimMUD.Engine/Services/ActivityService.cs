using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Main activity service that handles creatures' regeneration, temporary effects and hunting
	/// </summary>
	internal class ActivityService: BaseService
	{
		private readonly List<string> _toDelete = new List<string>();

		private void ProcessHunt(ExecutionContext ctx)
		{
			var now = DateTime.Now;
			if (!ctx.HuntInfo.IsActive || (now - ctx.HuntInfo.LastHunt).TotalMilliseconds < Configuration.HuntPauseInMs)
			{
				return;
			}

			var target = ctx.HuntInfo.Target;
			if (target.Room.Id == ctx.Room.Id)
			{
				// Found
				ctx.Send($"The hunt is over. You found {target.ShortDescription}.");
				ctx.HuntInfo.Reset();
			}
			else
			{
				if (ctx.HuntInfo.TargetRoomId != target.Room.Id)
				{
					// Target moved, rebuild path
					if (!ctx.HuntInfo.SetTarget(ctx.Room, target))
					{
						ctx.Send($"{target.ShortDescription} can't be reached. The hunt is over.");
						return;
					}
				}

				var pr = ctx.HuntInfo.GetForRoom(ctx.Room.Id);
				if (pr == null)
				{
					// Should never happen
					ctx.HuntInfo.Reset();
					ctx.Send($"{target.ShortDescription} can't be reached. The hunt is over.");
					return;
				}

				var moveSteps = pr.Value.RemainingSteps;

				Debug.WriteLine($"{ctx.ShortDescription} hunts {target.ShortDescription}. Remaining steps: {moveSteps}");

				string farAway;
				if (moveSteps > 20)
				{
					farAway = "very far away";
				}
				else if (moveSteps > 10)
				{
					farAway = "far away";
				}
				else if (moveSteps > 5)
				{
					farAway = "not so far away";
				}
				else if (moveSteps > 2)
				{
					farAway = "close";
				}
				else
				{
					farAway = "very close";
				}

				ctx.Send($"You continue to hunt {target.ShortDescription}. The target is {farAway}.");

				try
				{
					ctx.SuppressStopHuntOnMovement = true;

					switch (pr.Value.Direction)
					{
						case Direction.North:
							BaseCommand.North.Execute(ctx);
							break;
						case Direction.East:
							BaseCommand.East.Execute(ctx);
							break;
						case Direction.South:
							BaseCommand.South.Execute(ctx);
							break;
						case Direction.West:
							BaseCommand.West.Execute(ctx);
							break;
						case Direction.Up:
							BaseCommand.Up.Execute(ctx);
							break;
						case Direction.Down:
							BaseCommand.Down.Execute(ctx);
							break;
					}

					if (target.Room.Id == ctx.Room.Id)
					{
						// Found
						ctx.Send($"The hunt is over. You found {target.ShortDescription}.");
						ctx.HuntInfo.Reset();
					}
				}
				finally
				{
					ctx.SuppressStopHuntOnMovement = false;
				}

				ctx.HuntInfo.LastHunt = now;
			}
		}

		private bool ProcessRegen(ref int currentValue, int maxValue, ref float fractionalValue, int regenValue, float secondsPassed)
		{
			if (currentValue == maxValue)
			{
				return false;
			}

			float r;
			if (currentValue < maxValue)
			{
				r = regenValue * secondsPassed / 60.0f;
			}
			else
			{
				r = -Configuration.NegativeRegen * secondsPassed / 60.0f;
			}

			fractionalValue += r;
			if (Math.Abs(fractionalValue) > 1)
			{
				// Update real hp
				var hpUpdate = (int)fractionalValue;
				currentValue += hpUpdate;
				fractionalValue -= hpUpdate;
				if (currentValue >= maxValue)
				{
					// Full
					currentValue = maxValue;
					fractionalValue = 0;
				}
			}

			return true;
		}

		protected override void InternalUpdate(TimeSpan elapsed)
		{
			var now = DateTime.Now;
			var secondsPassed = (float)elapsed.TotalSeconds;

			// Process creature
			foreach (var creature in Creature.ActiveCreatures)
			{
				var ctx = creature.GetContext();

				// Hitpoints regen
				var currentValue = creature.State.Hitpoints;
				var fractionalValue = creature.State.FractionalHitpointsRegen;
				if (ProcessRegen(ref currentValue, creature.Stats.MaxHitpoints, ref fractionalValue,
					creature.Stats.GetHitpointsRegen(ctx.IsFighting), secondsPassed))
				{
					creature.State.Hitpoints = currentValue;
					creature.State.FractionalHitpointsRegen = fractionalValue;
				}

				// Mana regen
				currentValue = creature.State.Mana;
				fractionalValue = creature.State.FractionalManaRegen;
				if (ProcessRegen(ref currentValue, creature.Stats.MaxMana, ref fractionalValue,
					creature.Stats.GetManaRegen(ctx.IsFighting), secondsPassed))
				{
					creature.State.Mana = currentValue;
					creature.State.FractionalManaRegen = fractionalValue;
				}

				// Moves regen
				currentValue = creature.State.Moves;
				fractionalValue = creature.State.FractionalMovesRegen;
				if (ProcessRegen(ref currentValue, creature.Stats.MaxMoves, ref fractionalValue,
					creature.Stats.GetMovesRegen(ctx.IsFighting), secondsPassed))
				{
					creature.State.Moves = currentValue;
					creature.State.FractionalMovesRegen = fractionalValue;
				}

				// Remove expired effects
				_toDelete.Clear();
				foreach (var pair in creature.TemporaryAffects)
				{
					var ta = pair.Value;
					var passed = now - ta.Started;

					if (ta.DurationInSeconds > 2 * 60 && ta.DurationInSeconds - passed.TotalSeconds <= 60 && !ta.WarnedAboutToExpire)
					{
						// Warn that the affect is about to expire
						ctx.Send($"'{ta.Name}' is about to expire.");

						ta.WarnedAboutToExpire = true;
					}

					if (passed.TotalSeconds >= ta.DurationInSeconds)
					{
						_toDelete.Add(pair.Key);

						if (!string.IsNullOrEmpty(ta.MessageDeactivated))
						{
							ctx.Send(ta.MessageDeactivated);
						}
						else
						{
							ctx.Send($"'{ta.Name}' wears off.");
						}
					}
				}

				foreach (var key in _toDelete)
				{
					creature.RemoveTemporaryAffect(key);
				}

				// Hunting
				ProcessHunt(ctx);

				// Command queue
				ctx.ProcessCommandQueue();
			}
		}
	}
}
