using System.IO;
using Newtonsoft.Json;

namespace AbarimMUD.Utils
{
	public static class Serialization
	{
		public static T DeserializeFromFilePath<T>(this string path)
		{
			string data;
			using (var stream = new StreamReader(path))
			{
				data = stream.ReadToEnd();
			}

			return DeserializeFromString<T>(data);
		}

		public static T DeserializeFromString<T>(this string data)
		{
			var settings = new JsonSerializerSettings
			{
			};

			var result = JsonConvert.DeserializeObject<T>(data, settings);

			return result;
		}

		public static void SerializeToJSONFile<T>(this T obj, string path)
		{
			var data = obj.SerializeToJSON();

			using (var stream = new StreamWriter(path, false))
			{
				stream.Write(data);
			}
		}

		public static string SerializeToJSON<T>(this T obj)
		{
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			};

			var result = JsonConvert.SerializeObject(obj, Formatting.Indented, settings);

			return result;
		}

	}
}
