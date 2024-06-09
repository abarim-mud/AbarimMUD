using System.IO;
using System;

namespace AbarimMUD.Storage
{
	public class SingleFileStorage<KeyType, ItemType> : GenericBaseStorage<KeyType, ItemType> where ItemType : class
	{
		public string FileName { get; private set; }

		public override string Name => FileName;

		public SingleFileStorage(Func<ItemType, KeyType> keyGetter, string fileName, Func<KeyType, KeyType> keyConverter = null) : base(keyGetter, keyConverter)
		{
			FileName = fileName;
		}

		protected override void InternalLoad()
		{
			ClearCache();

			var path = Path.Combine(BaseFolder, FileName);
			if (!File.Exists(path))
			{
				LogDoesntExist(path);
				return;
			}

			var socials = JsonDeserializeFromFile<ItemType[]>(path);
			foreach (var social in socials)
			{
				AddToCache(social);
			}
		}

		public void Create(ItemType entity)
		{
			var key = GetKey(entity);
			if (GetByKey(key) != null)
			{
				throw new Exception($"Item with key '{key}' already exist.");
			}

			// Add to the cache
			AddToCache(entity);
		}

		public override void SaveAll()
		{
			EnsureFolder(BaseFolder);

			var path = Path.ChangeExtension(Path.Combine(BaseFolder, FileName), "json");

			JsonSerializeToFile(path, All);
		}

		public override void Remove(ItemType entity)
		{
			base.Remove(entity);

			// Save immediately after removal
			SaveAll();
		}
	}
}