using System.Text.Json;
using AbarimMUD.Utils;

namespace AbarimMUD.Storage
{
	public abstract class BaseStorage
	{
		public bool Loaded { get; private set; }

		public virtual string Name => GetType().Name;

		internal void Load()
		{
			Loaded = false;
			InternalLoad();

			Loaded = true;
		}

		protected abstract void InternalLoad();

		protected internal virtual void SetReferences()
		{
		}

		protected void Log(string message) => DataContext.Log(message);

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