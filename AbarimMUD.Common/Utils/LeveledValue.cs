using System;
using System.Globalization;

namespace AbarimMUD.Utils
{
	/// <summary>
	/// struct that represents a value that can be leveled up. It consists of a base value and a multiplier. The value is calculated as BaseValue + (Level - 1) * Multiplier.
	/// </summary>
	public struct LeveledValue
	{
		public int BaseValue;
		public float Multiplier;

		public LeveledValue(int baseValue, float multiplier)
		{
			BaseValue = baseValue;
			Multiplier = multiplier;
		}

		public static bool TryParse(string value, out LeveledValue range)
		{
			range = new LeveledValue();

			var parts = value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
				range = new LeveledValue(bas, 0);
				return true;
			}

			if (parts[1] == "level")
			{
				// Multiplier is 1
				range = new LeveledValue(bas, 1);
				return true;
			}

			parts = parts[1].Split('*', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			float? multiplier = null;
			for (var i = 0; i < parts.Length; ++i)
			{
				float m;
				if (float.TryParse(parts[i], CultureInfo.InvariantCulture, out m))
				{
					multiplier = m;
					break;
				}
			}

			if (multiplier == null)
			{
				return false;
			}

			range = new LeveledValue(bas, multiplier.Value);

			return true;
		}

		public static LeveledValue Parse(string value)
		{
			LeveledValue result;
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

			if (Multiplier.EpsilonEquals(1.0f))
			{
				return $"{BaseValue}+level";
			}

			return $"{BaseValue}+level*{Multiplier.FormatFloat()}";
		}

		public int GetValue(int level)
		{
			// Make the level zero-based
			--level;

			return BaseValue + (int)(level * Multiplier);
		}

		public static bool AreEqual(LeveledValue r1, LeveledValue r2)
		{
			return r1.BaseValue == r2.BaseValue && r1.Multiplier.EpsilonEquals(r2.Multiplier);
		}

		public static bool operator ==(LeveledValue r1, LeveledValue r2) => AreEqual(r1, r2);
		public static bool operator !=(LeveledValue r1, LeveledValue r2) => !AreEqual(r1, r2);

		public override bool Equals(object obj) => obj is LeveledValue range && AreEqual(this, range);

		public override int GetHashCode() => HashCode.Combine(BaseValue, Multiplier);
	}
}
