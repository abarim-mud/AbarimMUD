using System;
using System.Text;

namespace AbarimMUD
{
	public class ConsoleCommand
	{
		private readonly string _text;

		public static readonly ConsoleCommand NewLine = new ConsoleCommand("\n\r", false);
		public static readonly ConsoleCommand ColorClear = new ConsoleCommand("0");
		public static readonly ConsoleCommand Bold = new ConsoleCommand("1");
		public static readonly ConsoleCommand Faint = new ConsoleCommand("2");
		public static readonly ConsoleCommand Underline = new ConsoleCommand("4");
		public static readonly ConsoleCommand Blink = new ConsoleCommand("5");
		public static readonly ConsoleCommand Reverse = new ConsoleCommand("7");
		public static readonly ConsoleCommand ForeColorBlack = new ConsoleCommand("0;30");
		public static readonly ConsoleCommand ForeColorRed = new ConsoleCommand("0;31");
		public static readonly ConsoleCommand ForeColorGreen = new ConsoleCommand("0;32");
		public static readonly ConsoleCommand ForeColorYellow = new ConsoleCommand("0;33");
		public static readonly ConsoleCommand ForeColorBlue = new ConsoleCommand("0;34");
		public static readonly ConsoleCommand ForeColorMagenta = new ConsoleCommand("0;35");
		public static readonly ConsoleCommand ForeColorCyan = new ConsoleCommand("0;36");
		public static readonly ConsoleCommand ForeColorLightGrey = new ConsoleCommand("0;37");
		public static readonly ConsoleCommand ForeColorDarkGrey = new ConsoleCommand("1;30");
		public static readonly ConsoleCommand ForeColorLightRed = new ConsoleCommand("1;31");
		public static readonly ConsoleCommand ForeColorLightGreen = new ConsoleCommand("1;32");
		public static readonly ConsoleCommand ForeColorLightYellow = new ConsoleCommand("1;33");
		public static readonly ConsoleCommand ForeColorLightBlue = new ConsoleCommand("1;34");
		public static readonly ConsoleCommand ForeColorLightMagenta = new ConsoleCommand("1;35");
		public static readonly ConsoleCommand ForeColorLightCyan = new ConsoleCommand("1;36");
		public static readonly ConsoleCommand ForeColorWhite = new ConsoleCommand("1;37");
		public static readonly ConsoleCommand BgBlack = new ConsoleCommand("40");
		public static readonly ConsoleCommand BgRed = new ConsoleCommand("41");
		public static readonly ConsoleCommand BgGreen = new ConsoleCommand("42");
		public static readonly ConsoleCommand BgYellow = new ConsoleCommand("43");
		public static readonly ConsoleCommand BgBlue = new ConsoleCommand("44");
		public static readonly ConsoleCommand BgMagenta = new ConsoleCommand("45");
		public static readonly ConsoleCommand BgCyan = new ConsoleCommand("46");
		public static readonly ConsoleCommand BgWhite = new ConsoleCommand("47");

		public ConsoleCommand(string cmd, bool wrap)
		{
			if (wrap)
			{
				_text = Convert.ToChar(27) + "[" + cmd + "m";
			}
			else
			{
				_text = cmd;
			}
		}

		public ConsoleCommand(string cmd)
			: this(cmd, true)
		{
		}

		public override string ToString()
		{
			return _text;
		}
	}

	public static class ConsoleCommandUtils
	{
		public static void AddNewLine(this StringBuilder sb)
		{
			sb.Append(ConsoleCommand.NewLine);
		}

		public static void AddTextLine(this StringBuilder sb, string text)
		{
			sb.Append(text);
			sb.Append(ConsoleCommand.NewLine);
		}

		public static string FixNewLines(this string data)
		{
			// Firstly determine if line contains '\n' not followed by '\r'

			// Convert all '\n' to '\n\r'
			var toFix = false;
			for (var i = 0; i < data.Length; ++i)
			{
				if (data[i] == '\n' &&
					((i >= data.Length - 1) ||
					(data[i + 1] != '\r')))
				{
					toFix = true;
					break;
				}
			}

			if (!toFix)
			{
				// Doesnt need to be fixed, return original
				return data;
			}

			var sb = new StringBuilder(data.Length * 2);

			for (var i = 0; i < data.Length; ++i)
			{
				sb.Append(data[i]);

				if (data[i] == '\n' &&
					((i >= data.Length - 1) ||
					(data[i + 1] != '\r')))
				{
					sb.Append('\r');
				}
			}

			return sb.ToString();
		}
	}
}