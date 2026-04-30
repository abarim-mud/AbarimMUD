using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Service that handles the wandering of mobiles, i.e. the random movement of mobiles around their area when not fighting, based on a configured chance.
	/// </summary>
	internal class MobileWanderService : BaseService
	{
		private static readonly Dictionary<Direction, BaseCommand> _moveCommands = new Dictionary<Direction, BaseCommand>
		{
			[Direction.North] = BaseCommand.North,
			[Direction.East] = BaseCommand.East,
			[Direction.South] = BaseCommand.South,
			[Direction.West] = BaseCommand.West,
			[Direction.Up] = BaseCommand.Up,
			[Direction.Down] = BaseCommand.Down
		};

		public MobileWanderService()
		{
			IntervalInMilliseconds = 30 * 1000;
		}

		protected override void InternalUpdate(TimeSpan elapsed)
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var mobile = creature as MobileInstance;
				if (mobile == null || !mobile.IsAlive)
				{
					continue;
				}

				var ctx = mobile.GetContext();
				if (ctx.IsFighting)
				{
					continue;
				}

				if (mobile.Info.Flags.Contains(MobileFlags.Sentinel))
				{
					continue;
				}

				int rnd;
				if (!Utility.RollPercentage(Configuration.NpcWanderChance, out rnd))
				{
					continue;
				}

				var dir = RoomExit.AllDirections[Utility.RandomRange(0, RoomExit.AllDirections.Length - 1)];
				if (!mobile.Room.Exits.ContainsKey(dir))
				{
					// No exit in that direction
					continue;
				}

				var exit = mobile.Room.Exits[dir];
				if (exit.TargetRoom.Area.Id != mobile.Info.Area.Id)
				{
					// Different area
					continue;
				}

				_moveCommands[dir].Execute(ctx);
			}
		}
	}
}
