using System.IO;

namespace AbarimMUD.Storage
{
	public class CustomStorage<T> : BaseStorage where T : new()
	{
		private T _item;

		public string Folder => DataContext.Folder;

		public string Filename { get; private set; }

		public override string Name => Filename;

		public T Item => _item;

		public CustomStorage(string filename)
		{
			Filename = filename;
		}

		protected override void InternalLoad()
		{
			_item = DeserializeOrCreate(Filename);
		}

		private T DeserializeOrCreate(string fileName)
		{
			var path = Path.Combine(Folder, fileName);

			T result;
			if (!File.Exists(path))
			{
				Log($"'{path}' doesnt exist. Creating new {typeof(T).Name}.");
				result = new T();
			}
			else
			{
				result = JsonDeserializeFromFile<T>(path);
			}

			return result;
		}

		public void Save()
		{
			var path = Path.Combine(Folder, Filename);
			JsonSerializeToFile(path, Item);
		}
	}
}