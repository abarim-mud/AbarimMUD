using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace AbarimMUD.Storage
{
	public static class DataContext
	{
		private static DataContextSettings _settings;
		private static readonly List<BaseStorage> _storages = new List<BaseStorage>();

		public static string Folder
		{
			get
			{
				EnsureInitialized();

				return _settings.Folder;
			}
		}

		private static void Log(string message) => _settings.Log(message);

		public static void Initialize(string path, Action<string> log)
		{
			_settings = new DataContextSettings(path, log);

			Log($"Database initialized at folder '{_settings.Folder}'");
		}

		public static void Register(BaseStorage storage)
		{
			EnsureInitialized();
			if (_storages.Contains(storage))
			{
				return;
			}

			Log($"Registered data storage for {storage.Name}");
			_storages.Add(storage);
		}

		public static void Unregister(BaseStorage storage)
		{
			EnsureInitialized();
			if (!_storages.Contains(storage))
			{
				return;
			}

			Log($"Unregistered data storage for {storage.Name}");
			_storages.Remove(storage);
		}

		public static void Load()
		{
			EnsureInitialized();

			foreach (var storage in _storages)
			{
				storage.Load(_settings);
			}

			foreach (var storage in _storages)
			{
				Log($"Setting references for {storage.Name}");
				storage.SetReferences();
			}
		}

		private static void EnsureInitialized()
		{
			if (_settings == null)
			{
				throw new Exception($"Database isn't initialized. Call DataStorage.Initialize first.");
			}
		}
	}
}
