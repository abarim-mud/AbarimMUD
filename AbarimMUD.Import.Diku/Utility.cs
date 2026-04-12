using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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

		public static Mobile ToAmMobile(this DikuLoad.Data.Mobile mobile)
		{
			var level = mobile.Level;
			if (level < 1)
			{
				level = 1;
			}

			var gold = mobile.Wealth;
			if (gold == 0)
			{
				gold = mobile.Level * 100;
			}

			var result = new Mobile
			{
				Id = mobile.VNum,
				Keywords = mobile.Name.SplitByWhitespace().ToHashSet(),
				ShortDescription = mobile.ShortDescription,
				LongDescription = mobile.LongDescription,
				Description = mobile.Description,
				Sex = mobile.Sex.ParseEnum<Sex>(),
			};

			var attackType = AttackType.Hit;
			if (mobile.Attacks.Count > 0)
			{
				attackType = Enum.Parse<AttackType>(mobile.Attacks[0].AttackType, true);
			}

			result.SetAutoLevel(mobile.Level, attackType);

			foreach (var dikuFlag in mobile.Flags)
			{
				MobileFlags flag;
				if (Enum.TryParse(dikuFlag.ToString(), true, out flag))
				{
					result.Flags.Add(flag);
				}
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
				Credits = area.Credits,
				MinimumLevel = area.MinimumLevel,
				MaximumLevel = area.MaximumLevel
			};

			foreach (var room in area.Rooms)
			{
				result.Rooms.Add(room.ToAmRoom());
			}

			foreach (var mobile in area.Mobiles)
			{
				result.Mobiles.Add(mobile.ToAmMobile());
			}

			foreach(var mobileReset in area.Resets)
			{
				if (mobileReset.ResetType != DikuLoad.Data.AreaResetType.NPC)
				{
					continue;
				}

				var room = (from r in result.Rooms where r.Id == mobileReset.Value4 select r).FirstOrDefault();
				if (room == null)
				{
					continue;
				}

				var mobileSpawn = new MobileSpawn
				{
					Mobile = new Mobile
					{
						Id = mobileReset.MobileVNum
					}
				};

				room.MobileSpawns.Add(mobileSpawn);
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
			if (!Enum.TryParse(s, true, out result))
			{
				return defaultValue;
			}

			return result;
		}

		public static T ParseEnum<T, T2>(this T2 e, T defaultValue = default(T)) where T : struct
			where T2 : struct => e.ToString().ParseEnum(defaultValue);
	}
}
