using System.Text;

namespace AbarimMUD.Utils
{
	public static class HashUtils
	{
		public static string CalculateMD5Hash(string input)
		{
			// step 1, calculate MD5 hash from input
			var md5 = System.Security.Cryptography.MD5.Create();
			var inputBytes = Encoding.ASCII.GetBytes(input);
			var hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			var sb = new StringBuilder();
			foreach (var h in hash)
			{
				sb.Append(h.ToString("X2"));
			}

			return sb.ToString();
		}
	}
}