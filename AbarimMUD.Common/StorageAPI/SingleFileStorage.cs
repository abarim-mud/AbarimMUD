﻿using System.IO;
using System;
using AbarimMUD.StorageAPI;

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

			var path = Path.Combine(DataContext.Folder, FileName);
			if (!File.Exists(path))
			{
				LogDoesntExist(path);
				return;
			}

			var entities = JsonDeserializeFromFile<ItemType[]>(path);
			foreach (var entity in entities)
			{
				AddToCache(entity);
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
			EnsureFolder(DataContext.Folder);

			var path = Path.ChangeExtension(Path.Combine(DataContext.Folder, FileName), "json");

			var all = All;
			try
			{
				foreach(var entity in all)
				{
					var asSE = entity as ISerializationEvents;
					if (asSE != null)
					{
						asSE.OnSerializationStarted();
					}
				}

				JsonSerializeToFile(path, All);
			}
			finally
			{
				foreach (var entity in all)
				{
					var asSE = entity as ISerializationEvents;
					if (asSE != null)
					{
						asSE.OnSerializationEnded();
					}
				}
			}
		}

		public override void Remove(ItemType entity)
		{
			base.Remove(entity);

			// Save immediately after removal
			SaveAll();
		}
	}
}