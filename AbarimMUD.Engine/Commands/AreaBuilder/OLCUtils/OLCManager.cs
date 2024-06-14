using System.Collections.Generic;
using System;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using System.Collections;

namespace AbarimMUD.Commands.AreaBuilder.OLCUtils
{

	public interface IOLCStorage : IEnumerable<IStoredInFile>
	{
		IStoredInFile FindById(string id);
	}

	internal static class OLCManager
	{
		private class OLCRecord<EntityType> : IOLCStorage where EntityType : class, IStoredInFile
		{
			private readonly MultipleFilesStorageString<EntityType> _storage;

			public OLCRecord(MultipleFilesStorageString<EntityType> storage)
			{
				_storage = storage ?? throw new ArgumentNullException(nameof(storage));
			}

			public IStoredInFile FindById(string id)
			{
				return _storage.GetByKey(id);
			}

			public IEnumerator<IStoredInFile> GetEnumerator() => _storage.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => _storage.GetEnumerator();
		}

		private static readonly string[] _keys;
		private static readonly string _keysString;
		private static readonly Dictionary<string, IOLCStorage> _records = new Dictionary<string, IOLCStorage>();

		public static string[] Keys => _keys;

		public static string KeysString => _keysString;

		static OLCManager()
		{
			_records["race"] = new OLCRecord<Race>(Race.Storage);
			_records["class"] = new OLCRecord<GameClass>(GameClass.Storage);
			_records["item"] = new OLCRecord<Item>(Item.Storage);

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
