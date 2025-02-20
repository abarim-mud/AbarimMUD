using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
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
				SectorType = Enum.Parse<SectorType>(room.SectorType.ToString(), true),
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

			var at = AttackType.Hit;
			Enum.TryParse(mobile.AttackType, out at);

			var maximumArmor = Math.Max(mobile.ArmorClassBash, Math.Max(mobile.ArmorClassPierce, Math.Max(mobile.ArmorClassSlash, mobile.ArmorClassExotic)));
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
				Level = level,
				Sex = Enum.Parse<Sex>(mobile.Sex, true),
				Gold = gold,
				Hitpoints = mobile.HitDice.Average,
				Mana = mobile.ManaDice.Average,
				Armor = 100 - maximumArmor,
				AttacksCount = 1 + level / 8,
				AttackType = at,
				Hit = level * 5 + mobile.HitRoll * 10,
				DamageRange = mobile.DamageDice.ToValueRange(),
			};

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
			var result = new Area
			{
				Name = area.Name,
				Credits = area.Builders,
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

			return result;
		}
	}
}
