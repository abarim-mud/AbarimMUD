using System;

namespace AbarimMUD.Commands.Builder
{
	public class AreaStats : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var area = context.Room.Area;

			context.Send(area.ToString());
			context.Send("Respawn Time In Minutes: " + area.RespawnTimeInMinutes.FormatFloat());
			context.Send("Last spawn time: " + area.LastSpawn);

			var passed = (float)(DateTime.Now - area.LastSpawn).TotalSeconds;
			var toRespawn = (int)(area.RespawnTimeInMinutes * 60.0f - passed);

			var mins = (int)(toRespawn / 60);
			var secs = toRespawn % 60;
			context.Send($"Time to respawn: {mins:00}:{secs:00}");

			return true;
		}
	}
}
