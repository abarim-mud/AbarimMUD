using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Import.Diku
{
	internal static class Utility
	{
		public static ValueRange ToValueRange(this DikuLoad.Data.Dice dice) => new ValueRange(dice.Minimum, dice.Maximum);

		public static Direction ToAMDirection(this DikuLoad.Data.Direction dir) => (Direction)dir;

		public static Room ToAmRoom(this DikuLoad.Data.Room room)
		{
			var result = new Room
			{
				Id = room.VNum,
				Name = room.Name,
				Description = room.Description,
				SectorType = room.SectorType.ParseEnum<SectorType, DikuLoad.Data.SectorType>(),
			};

			foreach (var exit in room.Exits)
			{
				if (exit.Value == null || exit.Value.TargetRoom == null)
				{
					continue;
				}

				var roomExit = new RoomExit
				{
					Direction = exit.Key.ToAMDirection(),
					Tag = exit.Value.TargetRoom.VNum
				};

				result.Exits[roomExit.Direction] = roomExit;
			}

			return result;
		}

		public static Area ToAmArea(this DikuLoad.Data.Area area)
		{
			var id = area.Name.Replace(" ", string.Empty);
			id = char.ToLower(id[0]) + id.Substring(1);
			var result = new Area
			{
				Id = id,
				Name = area.Name,
				Credits = area.Builders,
				MinimumLevel = area.MinimumLevel,
				MaximumLevel = area.MaximumLevel
			};

			foreach (var room in area.Rooms)
			{
				result.Rooms.Add(room.ToAmRoom());
			}

			return result;
		}

		public static T ParseEnum<T>(this string s, T defaultValue = default(T)) where T : struct
		{
			if (string.IsNullOrEmpty(s))
			{
				return defaultValue;
			}

			T result;
			if (!Enum.TryParse<T>(s, true, out result))
			{
				return defaultValue;
			}

			return result;
		}

		public static T ParseEnum<T, T2>(this T2 e, T defaultValue = default(T)) where T : struct
			where T2 : struct => e.ToString().ParseEnum(defaultValue);
	}
}
