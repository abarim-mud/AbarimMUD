﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AbarimMUD.Storage
{
	public abstract class GenericBaseStorage<KeyType, ItemType> : BaseStorage, IEnumerable<ItemType> where ItemType : class
	{
		private readonly Dictionary<KeyType, ItemType> _cache = new Dictionary<KeyType, ItemType>();
		private readonly Func<ItemType, KeyType> _keyGetter;
		private readonly Func<KeyType, KeyType> _keyConverter;

		public int Count => _cache.Count;

		public ItemType[] All => _cache.Values.ToArray();

		internal GenericBaseStorage(Func<ItemType, KeyType> keyGetter, Func<KeyType, KeyType> keyConverter = null)
		{
			_keyGetter = keyGetter ?? throw new ArgumentNullException(nameof(keyGetter));
			_keyConverter = keyConverter;
		}

		protected KeyType ConvertKey(KeyType key) => _keyConverter == null ? key : _keyConverter(key);

		protected KeyType GetKey(ItemType entity, bool convert = true)
		{
			var key = _keyGetter(entity);

			if (convert)
			{
				key = ConvertKey(key);
			}

			return key;
		}

		protected void AddToCache(ItemType entity)
		{
			var key = GetKey(entity);
			_cache[key] = entity;
		}

		public ItemType GetByKey(KeyType key)
		{
			if (!Loaded)
			{
				throw new Exception($"{Name} isn't loaded.");
			}

			key = ConvertKey(key);
			ItemType result;
			if (!_cache.TryGetValue(key, out result))
			{
				return null;
			}

			return result;
		}

		public ItemType EnsureByKey(KeyType key)
		{
			var result = GetByKey(key);
			if (result == null)
			{
				throw new Exception($"Could not find item with id {key}");
			}

			return result;
		}

		public ItemType Lookup(string id)
		{
			if (!Loaded)
			{
				throw new Exception($"{Name} isn't loaded.");
			}

			foreach (var pair in _cache)
			{
				var name = pair.Key.ToString();
				if (name.StartsWith(id, StringComparison.CurrentCultureIgnoreCase))
				{
					return pair.Value;
				}
			}

			// Default to null on failed lookup.
			return null;
		}

		protected virtual void ClearCache() => _cache.Clear();

		protected void EnsureFolder(string folderPath)
		{
			if (!Directory.Exists(folderPath))
			{
				Log($"Creating folder '{folderPath}'");
				Directory.CreateDirectory(folderPath);
			}
		}

		public virtual void Remove(ItemType item)
		{
			var key = _keyGetter(item);
			_cache.Remove(key);
		}

		public abstract void SaveAll();

		public IEnumerator<ItemType> GetEnumerator() => _cache.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _cache.GetEnumerator();
	}
}