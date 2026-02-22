using System;

namespace AbarimMUD.Utils
{
	public struct ValueRange
	{
		private const float LogInterpolationBase = 7;

		public int Minimum;
		public int Maximum;

		public ValueRange(int min, int max)
		{
			Minimum = min;
			Maximum = max;
		}

		public override string ToString() => $"{Minimum}-{Maximum}";

		public int Random() => Utility.RandomRange(Minimum, Maximum);

		/// <summary>
		/// Calculates interpolated value for specified level
		/// Assuming that Minimum is level 1 value and Maximum is level 100 value
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int CalculateValue(int level)
		{
			if (level < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			// Log interpolation
			var k = (float)Math.Log(1 + (LogInterpolationBase - 1) * (level - 1) / 99.0f, LogInterpolationBase);
			var value = Minimum + k * (Maximum - Minimum);

			return (int)value;
		}

		public static bool TryParse(string value, out ValueRange range)
		{
			range = new ValueRange();

			var parts = value.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			if (parts.Length != 2)
			{
				return false;
			}

			int min;
			if (!int.TryParse(parts[0], out min))
			{
				return false;
			}

			int max;
			if (!int.TryParse(parts[1], out max))
			{
				return false;
			}

			range.Minimum = min;
			range.Maximum = max;

			return true;
		}

		public static ValueRange Parse(string value)
		{
			ValueRange result;
			if (!TryParse(value, out result))
			{
				throw new Exception($"Unable to parse ValueRange '{value}'");
			}

			return result;
		}

		public static ValueRange operator +(ValueRange r1, ValueRange r2)
		{
			return new ValueRange(r1.Minimum + r2.Minimum, r1.Maximum + r2.Maximum);
		}

		public static ValueRange operator +(ValueRange r1, int r2)
		{
			return new ValueRange(r1.Minimum + r2, r1.Maximum + r2);
		}

		public static ValueRange operator -(ValueRange r1, int r2)
		{
			return new ValueRange(r1.Minimum - r2, r1.Maximum - r2);
		}

		public static bool AreEqual(ValueRange r1, ValueRange r2)
		{
			return r1.Minimum == r2.Minimum && r1.Maximum == r2.Maximum;
		}

		public static bool operator ==(ValueRange r1, ValueRange r2) => AreEqual(r1, r2);
		public static bool operator !=(ValueRange r1, ValueRange r2) => !AreEqual(r1, r2);
	}
}
