using System.IO;
using System.Text.Json;

namespace AbarimMUD.Utils
{
	public static class Serialization
	{
		public static string SerializeToJSON<T>(this T obj)
		{
			var settings = new JsonSerializerOptions
			{
				WriteIndented = true,
			};

			var result = JsonSerializer.Serialize(obj, settings);

			return result;
		}
	}
}
