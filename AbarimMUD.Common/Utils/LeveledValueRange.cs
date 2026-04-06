using System;
using System.Globalization;

namespace AbarimMUD.Utils
{
	public struct LeveledValueRange
	{
		public int BaseValue;
		public float Multiplier;

		public LeveledValueRange(int baseValue, float multiplier)
		{
			BaseValue = baseValue;
			Multiplier = multiplier;
		}

		public static bool TryParse(string value, out LeveledValueRange range)
		{
			range = new LeveledValueRange();
			var parts = value.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			if (parts.Length < 1)
			{
				return false;
			}

			int bas;
			if (!int.TryParse(parts[0], out bas))
			{
				return false;
			}

			if (parts.Length == 1)
			{
				range = new LeveledValueRange(bas, 0);
				return true;
			}

			float multiplier;
			if (!float.TryParse(parts[1], CultureInfo.InvariantCulture, out multiplier))
			{
				return false;
			}

			range = new LeveledValueRange(bas, multiplier);

			return true;
		}

		public static LeveledValueRange Parse(string value)
		{
			LeveledValueRange result;
			if (!TryParse(value, out result))
			{
				throw new Exception($"Unable to parse LeveledValueRange '{value}'");
			}

			return result;
		}

		public override string ToString()
		{
			if (Multiplier.IsZero())
			{
				return BaseValue.ToString();
			}

			return $"{BaseValue}/{Multiplier.FormatFloat()}";
		}

		public int GetValue(int level)
		{
			// Make the level zero-based
			--level;

			return BaseValue + (int)(level * Multiplier);
		}

		public static bool AreEqual(LeveledValueRange r1, LeveledValueRange r2)
		{
			return r1.BaseValue == r2.BaseValue && r1.Multiplier.EpsilonEquals(r2.Multiplier);
		}

		public static bool operator ==(LeveledValueRange r1, LeveledValueRange r2) => AreEqual(r1, r2);
		public static bool operator !=(LeveledValueRange r1, LeveledValueRange r2) => !AreEqual(r1, r2);

		public override bool Equals(object obj) => obj is LeveledValueRange range && AreEqual(this, range);

		public override int GetHashCode() => HashCode.Combine(BaseValue, Multiplier);
	}
}
