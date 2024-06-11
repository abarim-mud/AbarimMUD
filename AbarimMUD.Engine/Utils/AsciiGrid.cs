using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AbarimMUD.Utils
{
	public class AsciiGrid
	{
		private static readonly Dictionary<int, string> _values = new Dictionary<int, string>();

		public int ColSpace { get; set; } = 2;

		private int GetKey(int x, int y) => y << 16 | x;

		private static Point KeyToPosition(int key) => new Point(key & 0xffff, key >> 16);

		public void SetValue(int x, int y, string value)
		{
			var key = GetKey(x, y);
			_values[key] = value;
		}

		public string GetValue(int x, int y)
		{
			var key = GetKey(x, y);
			string result;
			if (!_values.TryGetValue(key, out result))
			{
				return string.Empty;
			}

			return result;
		}

		public override string ToString()
		{
			// Determine max x and y
			var max = new Point(0, 0);
			foreach (var pair in _values)
			{
				var pos = KeyToPosition(pair.Key);
				if (pos.X > max.X)
				{
					max.X = pos.X;
				}

				if (pos.Y > max.Y)
				{
					max.Y = pos.Y;
				}
			}

			// Determine column widths
			var colWidths = new int[max.X + 1];
			foreach (var pair in _values)
			{
				var pos = KeyToPosition(pair.Key);
				var size = pair.Value != null ? pair.Value.Length : 0;
				if (size > colWidths[pos.X])
				{
					colWidths[pos.X] = size;
				}
			}

			var spaceString = string.Empty.PadRight(ColSpace);
			var sb = new StringBuilder();
			for(var y = 0; y <= max.Y; ++y)
			{
				for(var x = 0; x <= max.X; ++x)
				{
					var value = GetValue(x, y).PadRight(colWidths[x]);
					sb.Append(value);
					if (x < max.X)
					{
						sb.Append(spaceString);
					}
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
