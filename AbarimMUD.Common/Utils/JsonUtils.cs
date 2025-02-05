using System.Text.Json.Serialization;
using System.Text.Json;
using System;
using System.IO;
using System.Collections.Generic;

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

		private class RandomRangeConverterType: JsonConverter<ValueRange>
		{
			public override ValueRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var value = reader.GetString();
				var parts = value.Split('-');

				if (parts.Length != 2)
				{
					throw new Exception($"Unable to parse RandomRange '{value}'");
				}

				return new ValueRange(int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()));
			}

			public override void Write(Utf8JsonWriter writer, ValueRange value, JsonSerializerOptions options)
			{
				writer.WriteStringValue($"{value.Minimum}-{value.Maximum}");
			}
		}


		private static readonly LongConverterType LongConverter = new LongConverterType();
		private static readonly RandomRangeConverterType RandomRangeConverter = new RandomRangeConverterType();

		public static JsonSerializerOptions CreateOptions()
		{
			var result = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				IncludeFields = true,
				IgnoreReadOnlyFields = true,
				IgnoreReadOnlyProperties = true,
			};

			result.Converters.Add(new JsonStringEnumConverter());
			result.Converters.Add(LongConverter);
			result.Converters.Add(RandomRangeConverter);

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
