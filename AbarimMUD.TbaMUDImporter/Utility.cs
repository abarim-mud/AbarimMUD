using System;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;
using System.Threading.Tasks;
using System.Threading;

namespace AbarimMUD.TbaMUDImporter
{
	internal static class Utility
	{
		public static string RemoveTilda(this string str) => str.Replace("~", "");

		public static int ParseVnum(this string str) => int.Parse(str.Substring(1));

		public static long LoadFlags(string str)
		{
			long result = 0;
			var isNum = true;
			for(var i = 0; i < str.Length; i++)
			{
				var c = str[i];

				if(char.IsLower(c))
				{
					int value = c - 'a';
					result |= (long)1 << value;
				} else if (char.IsUpper(c))
				{
					int value = c - 'A' + 26;
					result |= (long)1 << value;
				}

				if (!char.IsDigit(c) && (c != '-' || i > 0))
				{
					isNum = false;
				}
			}

			if (isNum)
			{
				result = long.Parse(str);
			}

			return result;
		}

		public static string ReadDikuString(this StreamReader reader)
		{
			var result = new StringBuilder();

			while(!reader.EndOfStream)
			{
				var c = (char)reader.Read();

				if (c == '~')
				{
					// Skip new line
					reader.ReadLine();
					break;
				}

				result.Append(c);
			}

			return result.ToString();
		}
	}
}
