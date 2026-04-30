using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;
using System.Diagnostics;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Provides hunt processing services for active creatures, managing the progression and completion of hunt activities
	/// within the system.
	/// </summary>
	internal class HuntService: BaseService
	{
		public HuntService()
		{
			IntervalInMilliseconds = Configuration.HuntPauseInMs;
		}

		private void ProcessHunt(ExecutionContext ctx)
		{
			if (!ctx.HuntInfo.IsActive)
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
			}
		}

		protected override void InternalUpdate(TimeSpan elapsed)
		{
			// Process creature
			foreach (var creature in Creature.ActiveCreatures)
			{
				var ctx = creature.GetContext();

				// Hunting
				ProcessHunt(ctx);
			}
		}
	}
}
