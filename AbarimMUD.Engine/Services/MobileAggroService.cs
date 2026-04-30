using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;

namespace AbarimMUD.Services
{
	internal class MobileAggroService : BaseService
	{
		public MobileAggroService()
		{
			IntervalInMilliseconds = 2000;
		}

		protected override void InternalUpdate(TimeSpan elapsed)
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var mobile = creature as MobileInstance;
				if (mobile == null || !mobile.IsAlive || !mobile.Info.Flags.Contains(MobileFlags.Aggressive))
				{
					continue;
				}

				var ctx = mobile.GetContext();
				if (ctx.IsFighting)
				{
					continue;
				}

				foreach (var character in mobile.Room.Characters)
				{
					if (!character.IsAlive)
					{
						continue;
					}

					var characterCtx = character.GetContext();
					characterCtx.Send($"{mobile.ShortDescription} screams and attacks you!");
					characterCtx.SendRoomExceptMe($"{mobile.ShortDescription} screams attacks {character.ShortDescription}!");

					BaseCommand.Kill.Execute(ctx, character.Name);
					break;
				}
			}
		}
	}
}
