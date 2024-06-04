using System;
using System.IO;
using System.Linq;

namespace AbarimMUD.Storage
{
	public class MultipleFilesStorage<KeyType, ItemType> : GenericBaseStorage<KeyType, ItemType> where ItemType : StoredInFile
	{
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

			entity.Filename = Path.GetFileNameWithoutExtension(filePath);
			AddToCache(entity);

			return entity;
		}

		protected virtual void InternalSave(ItemType entity)
		{
			var path = BuildPath(entity);
			var folder = Path.GetDirectoryName(path);
			EnsureFolder(folder);
			JsonSerializeToFile(path, entity);
		}

		public void Save(ItemType entity)
		{
			var key = GetKey(entity);
			if (GetByKey(key) == null)
			{
				throw new Exception($"Item with key '{key}' doesn't exist. Use Create instead of Save");
			}

			InternalSave(entity);
		}

		public void Create(ItemType entity)
		{
			var key = GetKey(entity);
			if (GetByKey(key) != null)
			{
				throw new Exception($"Item with key '{key}' already exist.");
			}

			// Save the data
			InternalSave(entity);

			// Add to the cache
			AddToCache(entity);
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

		protected virtual string BuildPath(ItemType entity) => Path.ChangeExtension(Path.Combine(Folder, entity.Filename), "json");

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