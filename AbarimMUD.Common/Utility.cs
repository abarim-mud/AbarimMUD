using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD
{
	internal static class Utility
	{
		public static Random Random { get; } = new Random();

		public static bool RollPercentage(int percentage)
		{
			if (percentage < 1)
			{
				// Fail
				return false;
			}

			if (percentage >= 100)
			{
				// Win
				return true;
			}

			var rnd = Random.Next(1, 101);
			return rnd <= percentage;
		}

		public static int RandomRange(int min, int max) => Random.Next(min, max + 1);

		public static JsonSerializerOptions CreateDefaultOptions()
		{
			return new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			};
		}

		public static float Clamp(float val, float min = 0.0f, float max = 1.0f)
		{
			if (val < min)
			{
				return min;
			}

			if (val > max)
			{
				return max;
			}

			return val;
		}
	}
}
