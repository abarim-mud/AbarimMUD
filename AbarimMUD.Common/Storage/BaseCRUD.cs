using AbarimMUD.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Storage
{
	public abstract class BaseCRUD<T> : IEnumerable<T> where T : Entity
	{
		private readonly DataContextSettings _context;
		internal readonly Dictionary<string, T> _cache = new Dictionary<string, T>();

		internal string BaseFolder => _context.Folder;

		public int Count => _cache.Count;

		public T[] All => _cache.Values.ToArray();

		internal BaseCRUD(DataContextSettings context)
		{
			_context = context;
		}

		internal void Log(string message) => _context.Log(message);

		internal void AddToCache(T entity)
		{
			_cache[entity.Id] = entity;
		}

		public T GetById(string id)
		{
			T result;
			if (!_cache.TryGetValue(id, out result))
			{
				return null;
			}

			return result;
		}

		public T EnsureById(string id)
		{
			var result = GetById(id);
			if (result == null)
			{
				throw new Exception($"Could not find item with id {id}");
			}

			return result;
		}

		public IEnumerator<T> GetEnumerator() => _cache.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _cache.Values.GetEnumerator();
	}
}
