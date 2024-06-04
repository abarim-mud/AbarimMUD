using System;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public abstract class BaseStorage
	{
		private DataContextSettings _settings;

		protected string BaseFolder => _settings.Folder;
		public bool Loaded => _settings != null;
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

		protected void Log(string message) => _settings.Log(message);

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
