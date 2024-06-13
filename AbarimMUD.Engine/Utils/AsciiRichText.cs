using System.Collections.Generic;
using System;
using System.Reflection;
using System.Text;

namespace AbarimMUD.Utils
{
	public static class AsciiRichText
	{
		public static readonly string NewLine = "\n\r";
		public static readonly string Reset = Convert.ToChar(27) + "[0m";
		public static readonly string Bold = Convert.ToChar(27) + "[1m";
		public static readonly string Faint = Convert.ToChar(27) + "[2m";
		public static readonly string Underline = Convert.ToChar(27) + "[4m";
		public static readonly string Blink = Convert.ToChar(27) + "[5m";
		public static readonly string Reverse = Convert.ToChar(27) + "[7m";
		public static readonly string ForeColorBlack = Convert.ToChar(27) + "[0;30m";
		public static readonly string ForeColorRed = Convert.ToChar(27) + "[0;31m";
		public static readonly string ForeColorGreen = Convert.ToChar(27) + "[0;32m";
		public static readonly string ForeColorYellow = Convert.ToChar(27) + "[0;33m";
		public static readonly string ForeColorBlue = Convert.ToChar(27) + "[0;34m";
		public static readonly string ForeColorMagenta = Convert.ToChar(27) + "[0;35m";
		public static readonly string ForeColorCyan = Convert.ToChar(27) + "[0;36m";
		public static readonly string ForeColorLightGrey = Convert.ToChar(27) + "[0;37m";
		public static readonly string ForeColorDarkGrey = Convert.ToChar(27) + "[1;30m";
		public static readonly string ForeColorLightRed = Convert.ToChar(27) + "[1;31m";
		public static readonly string ForeColorLightGreen = Convert.ToChar(27) + "[1;32m";
		public static readonly string ForeColorLightYellow = Convert.ToChar(27) + "[1;33m";
		public static readonly string ForeColorLightBlue = Convert.ToChar(27) + "[1;34m";
		public static readonly string ForeColorLightMagenta = Convert.ToChar(27) + "[1;35m";
		public static readonly string ForeColorLightCyan = Convert.ToChar(27) + "[1;36m";
		public static readonly string ForeColorWhite = Convert.ToChar(27) + "[1;37m";
		public static readonly string BgBlack = Convert.ToChar(27) + "[40m";
		public static readonly string BgRed = Convert.ToChar(27) + "[41m";
		public static readonly string BgGreen = Convert.ToChar(27) + "[42m";
		public static readonly string BgYellow = Convert.ToChar(27) + "[43m";
		public static readonly string BgBlue = Convert.ToChar(27) + "[44m";
		public static readonly string BgMagenta = Convert.ToChar(27) + "[45m";
		public static readonly string BgCyan = Convert.ToChar(27) + "[46m";
		public static readonly string BgWhite = Convert.ToChar(27) + "[47m";

		private static readonly Dictionary<string, string> _commandsByNames = new Dictionary<string, string>();

		static AsciiRichText()
		{
			var staticFields = typeof(AsciiRichText).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in staticFields)
			{
				if (field.FieldType != typeof(string))
				{
					continue;
				}

				var name = field.Name.ToLower();
				if (name.StartsWith("forecolor"))
				{
					name = name.Substring(9);
				}

				_commandsByNames[name] = (string)field.GetValue(null);
			}
		}

		public static string Format(string source)
		{
			// First: determine whether formatting is required
			var needFormat = false;
			for (var i = 0; i < source.Length; ++i)
			{
				var c = source[i];

				if (c == '[')
				{
					needFormat = true;
					break;
				}
			}

			if (!needFormat)
			{
				return source;
			}

			var result = new StringBuilder();
			var commandBuffer = new StringBuilder();

			var isCommand = false;
			for (var i = 0; i < source.Length; ++i)
			{
				var c = source[i];

				if (!isCommand)
				{
					if (c != '[')
					{
						result.Append(c);
					}
					else
					{
						commandBuffer.Clear();
						isCommand = true;
					}
				}
				else
				{
					if (c != ']')
					{
						commandBuffer.Append(c);
					}
					else
					{
						string command;
						if (!_commandsByNames.TryGetValue(commandBuffer.ToString().ToLower(), out command))
						{
							// Unknown command
							// Restore the original value
							result.Append('[');
							result.Append(commandBuffer);
							result.Append(']');
						}
						else
						{
							result.Append(command);
						}

						isCommand = false;
					}
				}
			}

			return result.ToString();
		}
	}
}
