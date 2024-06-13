using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.Storage
{
	public class MultipleFilesStorage<KeyType, ItemType> : GenericBaseStorage<KeyType, ItemType> where ItemType : class
	{
		private readonly Dictionary<KeyType, string> _files = new Dictionary<KeyType, string>();

		private string SubfolderName { get; set; }

		public ItemType this[KeyType index] => EnsureByKey(index);
		public override string Name => SubfolderName;

		public string Folder => Path.Combine(BaseFolder, SubfolderName);

		public MultipleFilesStorage(Func<ItemType, KeyType> keyGetter, string subFolderName, Func<KeyType, KeyType> keyConverter = null) : base(keyGetter, keyConverter)
		{
			SubfolderName = subFolderName;
		}

		protected override void InternalLoad()
		{
			ClearCache();

			var filesList = ListFiles();
			foreach (var filePath in filesList)
			{
				LoadEntity(filePath);
			}
		}

		public virtual ItemType LoadEntity(string filePath)
		{
			var entity = JsonDeserializeFromFile<ItemType>(filePath);

			AddToCache(entity);
			
			_files[GetKey(entity)] = filePath;

			return entity;
		}

		protected virtual void InternalSave(ItemType entity)
		{
			var path = BuildPath(entity);
			var folder = Path.GetDirectoryName(path);
			EnsureFolder(folder);
			JsonSerializeToFile(path, entity);

			_files[GetKey(entity)] = path;
		}

		public void Save(ItemType entity)
		{
			var key = GetKey(entity);
			if (!Cache.ContainsKey(key))
			{
				// If the item isn't cached, check the possibility it was renamed
				KeyValuePair<KeyType, ItemType>? existing = null;
				foreach (var pair in Cache)
				{
					if (pair.Value == entity)
					{
						existing = pair;
						break;
					}
				}

				if (existing != null)
				{
					// It was renamed
					// Delete the file
					var existingKey = existing.Value.Key;
					string path;
					if (_files.TryGetValue(existingKey, out path))
					{
						File.Delete(path);
						_files.Remove(existingKey);
					} else
					{
						Log($"WARNING: Unable to find file for existing item {existingKey}");
					}

					// Remove from cache
					RemoveFromCache(existingKey);
				}
			}

			// Save the data
			InternalSave(entity);

			// Add to the cache
			AddToCache(entity);
		}

		public void Create(ItemType entity)
		{
			var key = GetKey(entity);
			if (GetByKey(key) != null)
			{
				throw new Exception($"Item with key '{key}' already exist.");
			}

			Save(entity);
		}

		protected virtual string[] ListFiles()
		{
			if (!Directory.Exists(Folder))
			{
				Log($"WARNING: Folder '{Folder}' doesnt exist.");
				return new string[0];
			}

			return Directory.EnumerateFiles(Folder, "*.json", SearchOption.AllDirectories).ToArray();

		}

		private string BuildPath(KeyType key) => Path.ChangeExtension(Path.Combine(Folder, key.ToString()), "json");

		protected virtual string BuildPath(ItemType entity) => BuildPath(GetKey(entity, false));

		public override void Remove(ItemType entity)
		{
			// Delete the file
			var path = BuildPath(entity);
			File.Delete(path);

			base.Remove(entity);
		}

		public override void SaveAll()
		{
			foreach (var item in this)
			{
				InternalSave(item);
			}
		}
	}
}