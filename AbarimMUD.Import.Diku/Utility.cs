using DikuLoad.Data;

namespace AbarimMUD.Import.Diku
{
	internal static class Utility
	{
		public static Data.Direction ToAMDirection(this Direction dir) => (Data.Direction)dir;

		public static Data.Room ToAMRoom(this Room room)
		{
			var result = new Data.Room
			{
				Id = room.VNum,
				Name = room.Name,
				Description = room.Description
			};

			foreach (var exit in room.Exits)
			{
				if (exit.Value == null || exit.Value.TargetRoom == null)
				{
					continue;
				}

				var roomExit = new Data.RoomExit
				{
					Direction = exit.Key.ToAMDirection(),
					Tag = exit.Value.TargetRoom.VNum
				};

				result.Exits[roomExit.Direction] = roomExit;
			}

			return result;
		}

		public static Data.Area ToMMBArea(this Area area)
		{
			var result = new Data.Area
			{
				Name = area.Name,
				Credits = area.Builders,
				MinimumLevel = area.MinimumLevel,
				MaximumLevel = area.MaximumLevel
			};

			foreach (var room in area.Rooms)
			{
				result.Rooms.Add(room.ToAMRoom());
			}

			return result;
		}
	}
}
