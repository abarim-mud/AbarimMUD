using AbarimMUD.Data;
using System;
using System.Text.Json;

namespace AbarimMUD.Site.Utility
{
	public static class Utils
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
