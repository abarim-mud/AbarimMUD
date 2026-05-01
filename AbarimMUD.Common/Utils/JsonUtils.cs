using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Utils
{
	internal static class JsonUtils
	{
		private static readonly LongConverterType LongConverter = new LongConverterType();
		private static readonly ValueRangeConverterType ValueRangeConverter = new ValueRangeConverterType();
		private static readonly LeveledValueConverterType LeveledValueConverter = new LeveledValueConverterType();

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
				var value = reader.GetString();

				return ValueRange.Parse(value);
			}

			public override void Write(Utf8JsonWriter writer, ValueRange value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.ToString());
			}
		}

		private class LeveledValueConverterType : JsonConverter<LeveledValue>
		{
			public override LeveledValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var value = reader.GetString();

				return LeveledValue.Parse(value);
			}

			public override void Write(Utf8JsonWriter writer, LeveledValue value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(value.ToString());
			}
		}

		public static JsonSerializerOptions CreateOptions()
		{
			var result = UrContext.CreateDefaultOptions();

			result.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

			result.Converters.Add(LongConverter);
			result.Converters.Add(ValueRangeConverter);
			result.Converters.Add(LeveledValueConverter);

			return result;
		}

		public static JsonSerializerOptions CreateCopyWithout(this JsonSerializerOptions options, JsonConverter converterToRemove)
		{
			var result = new JsonSerializerOptions(options);
			result.Converters.Remove(converterToRemove);

			return result;
		}
	}
}