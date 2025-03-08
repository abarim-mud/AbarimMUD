using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AbarimMUD.Utils
{
	public class AsciiGrid
	{
		private readonly Dictionary<int, string> _headers = new Dictionary<int, string>();
		private readonly Dictionary<int, string> _values = new Dictionary<int, string>();

		public int ColSpace { get; set; } = 2;

		private int GetKey(int x, int y) => y << 16 | x;

		private static Point KeyToPosition(int key) => new Point(key & 0xffff, key >> 16);

		public void SetHeader(int x, string value)
		{
			_headers[x] = value;
		}

		public void SetValue(int x, int y, string value)
		{
			var key = GetKey(x, y);
			_values[key] = value;
		}

		public string GetValue(int x, int y)
		{
			var key = GetKey(x, y);
			string result;
			if (!_values.TryGetValue(key, out result) || result == null)
			{
				return string.Empty;
			}

			return result;
		}

		public override string ToString()
		{
			// Determine max x and y
			var max = new Point(0, 0);

			foreach (var pair in _headers)
			{
				if (pair.Key > max.X)
				{
					max.X = pair.Key;
				}
			}

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
			foreach (var pair in _headers)
			{
				var size = pair.Value != null ? pair.Value.Length : 0;

				if (size > colWidths[pair.Key])
				{
					colWidths[pair.Key] = size;
				}
			}

			foreach (var pair in _values)
			{
				var pos = KeyToPosition(pair.Key);
				var size = pair.Value != null ? pair.Value.Length : 0;
				if (size > colWidths[pos.X])
				{
					colWidths[pos.X] = size;
				}
			}

			var totalWidth = 0;
			for (var i = 0; i < colWidths.Length; ++i)
			{
				totalWidth += colWidths[i];

				if (i < colWidths.Length - 1)
				{
					totalWidth += ColSpace;
				}

			}

			var spaceString = string.Empty.PadRight(ColSpace);
			var sb = new StringBuilder();

			// Headers
			if (_headers.Count > 0)
			{
				for (var x = 0; x <= max.X; ++x)
				{
					var value = string.Empty;

					string header;
					if (_headers.TryGetValue(x, out header))
					{
						value = header;
					}

					value = value.PadRight(colWidths[x]);
					sb.Append(value);
					if (x < max.X)
					{
						sb.Append(spaceString);
					}
				}

				sb.AppendLine();
				sb.AppendLine(new string('-', totalWidth));
			}

			// Data
			for (var y = 0; y <= max.Y; ++y)
			{
				for (var x = 0; x <= max.X; ++x)
				{
					var value = GetValue(x, y);
					value = value.PadRight(colWidths[x]);
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
