using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public abstract class BaseStorage
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

		private DataContextSettings _settings;

		private DataContextSettings Settings
		{
			get
			{
				if (_settings == null)
				{
					throw new Exception($"Settings are null. Make sure the DataContext.Load was called.");
				}

				return _settings;
			}
		}

		protected string BaseFolder => Settings.Folder;
		public bool Loaded => Settings != null;
		public virtual string Name => GetType().Name;

		internal void Load(DataContextSettings settings)
		{
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));

			InternalLoad();
		}

		protected abstract void InternalLoad();

		protected internal virtual void SetReferences()
		{
		}

		protected void Log(string message) => Settings.Log(message);

		protected void LogDoesntExist(string name)
		{
			Log($"WARNING: Folder '{name}' doesnt exist. Skipping loading of {Name}.");
		}

		protected virtual JsonSerializerOptions CreateJsonOptions()
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

		protected virtual void JsonSerializeToFile<T>(string path, T data)
		{
			Log($"Saving to '{path}'");

			var options = CreateJsonOptions();
			var s = JsonSerializer.Serialize(data, options);
			File.WriteAllText(path, s);
		}

		protected virtual T JsonDeserializeFromFile<T>(string path)
		{
			Log($"Loading '{path}'");

			var data = File.ReadAllText(path);
			var options = CreateJsonOptions();
			return JsonSerializer.Deserialize<T>(data, options);
		}
	}
}
