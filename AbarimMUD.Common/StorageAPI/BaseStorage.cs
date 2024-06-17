using System;
using System.Text.Json;
using AbarimMUD.Utils;

namespace AbarimMUD.Storage
{
	public abstract class BaseStorage
	{
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

		protected virtual JsonSerializerOptions CreateJsonOptions() => JsonUtils.CreateOptions();

		protected virtual void JsonSerializeToFile<T>(string path, T data)
		{
			Log($"Saving to '{path}'");

			JsonUtils.SerializeToFile(path, CreateJsonOptions(), data);
		}

		protected virtual T JsonDeserializeFromFile<T>(string path)
		{
			Log($"Loading '{path}'");

			return JsonUtils.DeserializeFromFile<T>(path, CreateJsonOptions());
		}
	}
}