using AbarimMUD.Data;
using MUDMapBuilder;
using System;
using System.Text.Json;

namespace AbarimMUD.Site.Utility
{
	public static class Utils
	{
		public static string SerializeToJSON<T>(this T obj)
		{
			var settings = new JsonSerializerOptions
			{
				WriteIndented = true,
			};

			var result = JsonSerializer.Serialize(obj, settings);

			return result;
		}

		public static MMBDirection ToMBBDirection(this Direction direction)
		{
			switch (direction)
			{
				case Direction.North:
					return MMBDirection.North;
				case Direction.East:
					return MMBDirection.East;
				case Direction.South:
					return MMBDirection.South;
				case Direction.West:
					return MMBDirection.West;
				case Direction.Up:
					return MMBDirection.Up;
				case Direction.Down:
					return MMBDirection.Down;
			}

			throw new Exception($"Unknown direction {direction}");
		}
	}
}
