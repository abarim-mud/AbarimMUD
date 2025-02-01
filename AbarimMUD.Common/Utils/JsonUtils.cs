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

		private class ValueRangeConverterType : JsonConverter<ValueRange>
		{
			public override ValueRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var doc = JsonDocument.ParseValue(ref reader);
				var values = JsonSerializer.Deserialize<SortedDictionary<int, int>>(doc);

				return new ValueRange(values);
			}

			public override void Write(Utf8JsonWriter writer, ValueRange value, JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value.Values);
			}
		}

		private static readonly LongConverterType LongConverter = new LongConverterType();
		private static readonly ValueRangeConverterType ValueRangeConverter = new ValueRangeConverterType();

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
			result.Converters.Add(ValueRangeConverter);

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
