using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Utils
{
	public class ValueRange
	{
		private readonly SortedDictionary<int, int> _values;

		public IReadOnlyDictionary<int, int> Values => _values;

		public ValueRange(SortedDictionary<int, int> values)
		{
			// Validate values
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}

			if (values.Count < 2)
			{
				throw new ArgumentOutOfRangeException("ValueRange dictionary should contain at least 2 values.");
			}

			if (values.First().Key != 1)
			{
				throw new ArgumentOutOfRangeException($"ValueRange first value key should equal to 1(first level). It is equal to {values.First().Key}.");
			}

			int? lastKey = null;
			foreach (var pair in values)
			{
				if (lastKey != null && lastKey.Value == pair.Key)
				{
					throw new ArgumentOutOfRangeException($"ValueRange dublicate key {lastKey.Value}.");
				}

				lastKey = pair.Key;
			}

			_values = values;
		}

		public ValueRange(int level1Value, int level100Value)
		{
			_values = new SortedDictionary<int, int>
			{
				[1] = level1Value,
				[100] = level100Value
			};
		}

		public int CalculateValue(int level)
		{
			if (level < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			var firstValueLevel = 1;
			var secondValueLevel = 100;
			var firstValue = 0;
			var secondValue = 0;
			var i = 0;
			foreach (var pair in Values)
			{
				if (level == pair.Key)
				{
					return pair.Value;
				}

				secondValueLevel = pair.Key;
				secondValue = pair.Value;

				if (level < pair.Key)
				{
					break;
				}

				if (i < Values.Count - 1)
				{
					firstValueLevel = pair.Key;
					firstValue = pair.Value;
				}

				++i;
			}

			// Simple linear interpolation
			var k = (float)(level - 1) / (secondValueLevel - firstValueLevel);
			var value = firstValue + k * (secondValue - firstValue);

			return (int)value;
		}

		public override bool Equals(object obj)
		{
			return obj is ValueRange && base.Equals((ValueRange)obj);
		}

		public bool Equals(ValueRange obj)
		{
			if (obj is null)
			{
				return false;
			}

			if (Values.Count != obj.Values.Count)
			{
				return false;
			}

			foreach (var pair in Values)
			{
				int v;
				if (!obj.Values.TryGetValue(pair.Key, out v))
				{
					return false;
				}

				if (pair.Value != v)
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator ==(ValueRange left, ValueRange right)
		{
			if (left is null && right is null)
			{
				return true;
			}

			if (left is null || right is null)
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(ValueRange left, ValueRange right)
		{
			return !(left == right);
		}
	}
}
