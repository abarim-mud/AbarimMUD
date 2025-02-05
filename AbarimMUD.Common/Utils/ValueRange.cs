using System;

namespace AbarimMUD.Utils
{
	public struct ValueRange
	{
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

			// Simple linear interpolation
			var k = (level - 1) / 99.0f;
			var value = Minimum + k * (Maximum - Minimum);

			return (int)value;
		}

		public static ValueRange operator +(ValueRange r1, ValueRange r2)
		{
			return new ValueRange(r1.Minimum + r2.Minimum, r1.Maximum + r2.Maximum);
		}

		public static bool AreEqual(ValueRange r1, ValueRange r2)
		{
			return r1.Minimum == r2.Minimum && r1.Maximum == r2.Maximum;
		}

		public static bool operator ==(ValueRange r1, ValueRange r2) => AreEqual(r1, r2);
		public static bool operator !=(ValueRange r1, ValueRange r2) => !AreEqual(r1, r2);
	}
}
