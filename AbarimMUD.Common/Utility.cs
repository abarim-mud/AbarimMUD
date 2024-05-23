using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD
{
	internal static class Utility
	{
		public static JsonSerializerOptions CreateDefaultOptions()
		{
			return new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
			};
		}
	}
}
