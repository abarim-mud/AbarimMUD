using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Main activity service that handles creatures' regeneration and effect
	/// </summary>
	internal class ActivityService: BaseService
	{
		private readonly List<string> _toDelete = new List<string>();

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

				// Command queue
				ctx.ProcessCommandQueue();
			}
		}
	}
}
