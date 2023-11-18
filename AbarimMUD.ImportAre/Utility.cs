using AbarimMUD.Common.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AbarimMUD.ImportAre
{
	internal static class Utility
	{
		public static string RemoveTilda(this string str) => str.Replace("~", "");

		public static int ParseVnum(this string str) => int.Parse(str.Substring(1));


		public static bool EndOfStream(this Stream stream) => stream.Position >= stream.Length;

		public static void GoBackIfNotEOF(this Stream stream)
		{
			if (stream.Position == 0 || stream.EndOfStream())
			{
				return;
			}

			stream.Seek(-1, SeekOrigin.Current);
		}

		public static char ReadLetter(this Stream stream) => (char)stream.ReadByte();

		public static char ReadSpacedLetter(this Stream stream)
		{
			char c;
			do
			{
				c = stream.ReadLetter();
			}
			while (!stream.EndOfStream() && char.IsWhiteSpace(c));

			return c;
		}

		public static string ReadLine(this Stream stream)
		{
			var sb = new StringBuilder();

			var endOfLine = false;
			while(!stream.EndOfStream())
			{
				var c = stream.ReadLetter();

				var isNewLine = c == '\n' || c == '\r';
				if (!endOfLine)
				{
					if (!isNewLine)
					{
						sb.Append(c);
					} else
					{
						endOfLine = true;
					}
				} else if (!isNewLine)
				{
					stream.GoBackIfNotEOF();
					break;
				}

			}

			return sb.ToString();
		}


		public static string ReadId(this Stream stream)
		{
			var c = stream.ReadSpacedLetter();
			if (c != '#')
			{
				throw new Exception("# not found");
			}

			return stream.ReadLine();
		}

		public static int LoadFlags(string str)
		{
			int result = 0;
			var isNum = true;
			for (var i = 0; i < str.Length; i++)
			{
				var c = str[i];

				if (char.IsLower(c))
				{
					int value = c - 'a';
					result |= (int)1 << value;
				}
				else if (char.IsUpper(c))
				{
					int value = c - 'A' + 26;
					result |= (int)1 << value;
				}

				if (!char.IsDigit(c) && (c != '-' || i > 0))
				{
					isNum = false;
				}
			}

			if (isNum)
			{
				result = int.Parse(str);
			}

			return result;
		}

		public static int ReadFlag(this Stream stream)
		{
			int result = 0;
			var negative = false;
			var c = stream.ReadSpacedLetter();

			if (c == '-')
			{
				negative = true;
				c = stream.ReadLetter();
			}

			if (!char.IsDigit(c))
			{
				while (!stream.EndOfStream())
				{
					int bitsum = 0;
					if ('A' <= c && c <= 'Z')
					{
						bitsum = 1;
						for (int i = c; i > 'A'; i--)
						{
							bitsum *= 2;
						}
					}
					else if ('a' <= c && c <= 'z')
					{
						bitsum = 67108864;
						for (int i = c; i > 'a'; i--)
						{
							bitsum *= 2;
						}
					}
					else
					{
						break;
					}

					result += bitsum;
					c = stream.ReadLetter();
				}
			}
			else
			{
				var sb = new StringBuilder();

				while (!stream.EndOfStream() && char.IsDigit(c))
				{
					sb.Append(c);
					c = stream.ReadLetter();
				}

				result = int.Parse(sb.ToString());
			}

			if (c == '|')
			{
				result += stream.ReadFlag();
			}

			// Last symbol beint to the new data
			stream.GoBackIfNotEOF();

			return negative ? -result : result;
		}

		public static int ReadNumber(this Stream stream)
		{
			var negative = false;
			var c = stream.ReadSpacedLetter();

			if (c == '+')
			{
				c = stream.ReadLetter();
			} else if (c == '-')
			{
				negative = true;
				c = stream.ReadLetter();
			}

			if (!char.IsDigit(c))
			{
				throw new Exception($"Could not parse number {c}");
			}

			var sb = new StringBuilder();

			while (!stream.EndOfStream() && char.IsDigit(c))
			{
				sb.Append(c);
				c = stream.ReadLetter();
			}

			var result = int.Parse(sb.ToString());

			if (negative)
			{
				result = -result;
			}

			if (c == '|')
			{
				result += stream.ReadNumber();
			}

			// Last symbol beint to the new data
			stream.GoBackIfNotEOF();

			return result;
		}


		public static string ReadDikuString(this Stream stream)
		{
			var result = new StringBuilder();

			while (!stream.EndOfStream())
			{
				var c = stream.ReadLetter();

				if (c == '~')
				{
					// Skip new line
					stream.ReadLine();
					break;
				}

				result.Append(c);
			}

			return result.ToString();
		}

		public static char EnsureChar(this Stream stream, char expected)
		{
			var c = stream.ReadLetter();
			if (c != expected)
			{
				throw new Exception($"Expected symbol '{expected}'");
			}

			return c;
		}

		public static string ReadDice(this Stream stream)
		{
			var sb = new StringBuilder();
			sb.Append(stream.ReadNumber());
			sb.Append(stream.EnsureChar('d'));
			sb.Append(stream.ReadNumber());
			sb.Append(stream.EnsureChar('+'));
			sb.Append(stream.ReadNumber());

			return sb.ToString();
		}

		public static string ReadWord(this Stream stream)
		{
			if (stream.EndOfStream())
			{
				return string.Empty;
			}

			var sb = new StringBuilder();
			var c = stream.ReadSpacedLetter();

			var startsWithQuote = c == '"' || c == '\'';

			if (startsWithQuote)
			{
				c = stream.ReadLetter();
			}

			while(!stream.EndOfStream())
			{
				if ((startsWithQuote && (c == '"' || c == '\'')) ||
					(!startsWithQuote && char.IsWhiteSpace(c)))
				{
					break;
				}

				sb.Append(c);
				c = stream.ReadLetter();
			}

			if (!startsWithQuote)
			{
				stream.GoBackIfNotEOF();
			}

			return sb.ToString();
		}

		public static T ToEnum<T>(this string value)
		{
			value = value.Replace("_", "").Replace(" ", "");

			// Parse the enum
			try
			{
				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch(Exception)
			{
				Importer.Log($"Enum parse error: enum type={typeof(T).Name}, value={value}");
				throw;
			}
		}

		public static T ReadEnumFromDikuString<T>(this Stream stream)
		{
			var str = stream.ReadDikuString();
			return str.ToEnum<T>();
		}

		public static T ReadEnumFromWord<T>(this Stream stream)
		{
			var word = stream.ReadWord();
			return word.ToEnum<T>();
		}

		public static Skill ReadSkill(this Stream stream)
		{
			var word = stream.ReadWord();
			if (string.IsNullOrEmpty(word))
			{
				return Skill.Reserved;
			}

			return word.ToEnum<Skill>();
		}
	}
}
