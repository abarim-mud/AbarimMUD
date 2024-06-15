﻿using System.Collections.Generic;
using System;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using System.Collections;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	public interface IOLCStorage : IEnumerable
	{
		bool RequiresId { get; }
		Type ObjectType { get; }

		object FindById(ExecutionContext context, string id);
		IEnumerable<object> Lookup(ExecutionContext context, string pattern);
	}

	internal static class OLCManager
	{
		private class OLCRecordString<EntityType> : IOLCStorage where EntityType : class, IStoredInFile
		{
			private readonly GenericBaseStorage<string, EntityType> _storage;

			public Type ObjectType => typeof(EntityType);

			public bool RequiresId => true;

			public OLCRecordString(GenericBaseStorage<string, EntityType> storage)
			{
				_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			}

			public object FindById(ExecutionContext context, string id) => _storage.GetByKey(id);

			public IEnumerable<object> Lookup(ExecutionContext context, string pattern)
			{
				foreach (var entity in _storage)
				{
					if (!string.IsNullOrEmpty(pattern) &&
						!entity.Id.Contains(pattern, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					yield return entity;
				}
			}

			public IEnumerator GetEnumerator() => _storage.GetEnumerator();
		}

		private class OLCRecordInt<EntityType> : IOLCStorage where EntityType : class
		{
			private readonly Func<IReadOnlyDictionary<int, EntityType>> _dictGetter;
			private readonly Func<EntityType, string> _nameGetter;

			public Type ObjectType => typeof(EntityType);
			public bool RequiresId => false;

			public OLCRecordInt(Func<IReadOnlyDictionary<int, EntityType>> dictGetter,
				Func<EntityType, string> nameGetter)
			{
				_dictGetter = dictGetter ?? throw new ArgumentNullException(nameof(dictGetter));
				_nameGetter = nameGetter ?? throw new ArgumentNullException(nameof(nameGetter));
			}

			public object FindById(ExecutionContext context, string id)
			{
				int num;
				if (!context.EnsureInt(id, out num))
				{
					return null;
				}

				var dict = _dictGetter();

				EntityType obj;
				if (!dict.TryGetValue(num, out obj))
				{
					return null;
				}

				return obj;
			}

			public IEnumerator GetEnumerator()
			{
				var dict = _dictGetter();

				return dict.Values.GetEnumerator();
			}

			public IEnumerable<object> Lookup(ExecutionContext context, string pattern)
			{
				pattern = pattern.Trim();

				var isNum = false;
				int num;

				if (int.TryParse(pattern, out num))
				{
					isNum = true;
				}

				var dict = _dictGetter();
				foreach (var pair in dict)
				{
					if (!string.IsNullOrEmpty(pattern))
					{
						if (isNum)
						{
							if (!pair.Key.ToString().Contains(pattern))
							{
								// Filter by id
								continue;
							}
						}
						else
						{
							var name = _nameGetter(pair.Value);
							if (!name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
							{
								// Filter by name
								continue;
							}
						}
					}

					yield return pair.Value;
				}
			}
		}


		private static readonly string[] _keys;
		private static readonly string _keysString;
		private static readonly Dictionary<string, IOLCStorage> _records = new Dictionary<string, IOLCStorage>();

		public static string[] Keys => _keys;

		public static string KeysString => _keysString;

		static OLCManager()
		{
			_records["class"] = new OLCRecordString<GameClass>(GameClass.Storage);
			_records["item"] = new OLCRecordString<Item>(Item.Storage);
			_records["character"] = new OLCRecordString<Character>(Character.Storage);
			_records["mobile"] = new OLCRecordInt<Mobile>(() => Area.Storage.AllMobiles, m => m.ShortDescription);
			_records["room"] = new OLCRecordInt<Room>(() => Area.Storage.AllRooms, r => r.Name);

			_keys = _records.Keys.ToArray();
			_keysString = string.Join('|', _keys);
		}

		public static IOLCStorage GetStorage(string key)
		{
			IOLCStorage result;
			if (!_records.TryGetValue(key, out result))
			{
				return null;
			}

			return result;
		}
	}
}