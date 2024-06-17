using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.IO;

namespace AbarimMUD.Utils
{
	public static class JsonUtils
	{
		private class LongConverterType : JsonConverter<long>
		{
			public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					return reader.GetString().ParseBigNumber();
				}

				return reader.GetInt64();
			}

			public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.FormatBigNumber());
			}
		}

		private static readonly LongConverterType LongConverter = new LongConverterType();

		public static JsonSerializerOptions CreateOptions()
		{
			var result = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.Never,
				IncludeFields = true,
				IgnoreReadOnlyFields = true,
				IgnoreReadOnlyProperties = true,
			};

			result.Converters.Add(new JsonStringEnumConverter());
			result.Converters.Add(LongConverter);

			return result;
		}

		public static void SerializeToFile<T>(string path, JsonSerializerOptions options, T data)
		{
			var s = JsonSerializer.Serialize(data, options);
			File.WriteAllText(path, s);
		}

		public static T DeserializeFromFile<T>(string path, JsonSerializerOptions options)
		{
			var data = File.ReadAllText(path);
			return JsonSerializer.Deserialize<T>(data, options);
		}
	}
}
