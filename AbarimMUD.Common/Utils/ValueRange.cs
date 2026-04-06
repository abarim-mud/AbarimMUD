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

		public override bool Equals(object obj) => obj is ValueRange range && AreEqual(this, range);
		public override int GetHashCode() => HashCode.Combine(Minimum, Maximum);
	}
}
