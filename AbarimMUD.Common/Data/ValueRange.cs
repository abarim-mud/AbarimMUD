using System;

namespace AbarimMUD.Data
{
	public struct ValueRange
	{
		public int Level1Value;
		public int Level100Value;

		public ValueRange(int level1Value, int level100Value)
		{
			Level1Value = level1Value;
			Level100Value = level100Value;
		}

		public int CalculateValue(int level)
		{
			if (level < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			if (level == 1)
			{
				return Level1Value;
			}

			if (level == 100)
			{
				return Level100Value;
			}

			// Sqrt interpolation
			var k = (float)Math.Sqrt((level - 1) / 99.0f);
			var value = Level1Value + k * (Level100Value - Level1Value);

			return (int)value;
		}

		public override bool Equals(object obj)
		{
			return obj is ValueRange && base.Equals((ValueRange)obj);
		}

		public bool Equals(ValueRange obj)
		{
			return Level1Value == obj.Level1Value && Level100Value == obj.Level100Value;
		}

		public static bool operator ==(ValueRange left, ValueRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(ValueRange left, ValueRange right)
		{
			return !left.Equals(right);
		}
	}
}
