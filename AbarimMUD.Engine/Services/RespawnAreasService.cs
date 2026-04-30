using AbarimMUD.Data;
using System;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Service that handles areas' respawn, i.e. the repop of mobs and items in the area after a certain time has passed since the last spawn.
	/// </summary>
	internal class RespawnAreasService : BaseService
	{
		protected override void InternalUpdate(TimeSpan elapsed)
		{
			var now = DateTime.Now;

			// Areas' repop
			foreach (var area in Area.Storage)
			{
				var passed = (float)(now - area.LastSpawn).TotalMinutes;

				if (passed > area.RespawnTimeInMinutes)
				{
					Server.Instance.SpawnArea(area);
				}
			}
		}
	}
}
