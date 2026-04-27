using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;

namespace AbarimMUD
{
	public class AIService
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

		private DateTime? _lastWanderDt;

		private void UpdateWander()
		{
			var now = DateTime.Now;
			if (_lastWanderDt == null)
			{
				_lastWanderDt = now;
				return;
			}

			if ((now - _lastWanderDt.Value).TotalSeconds < 30)
			{
				return;
			}

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

			_lastWanderDt = now;
		}

		public void Update()
		{
			UpdateWander();
		}
	}
}
