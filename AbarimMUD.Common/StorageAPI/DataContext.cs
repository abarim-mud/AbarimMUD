using System;
using System.Collections.Generic;

namespace AbarimMUD.Storage
{
	public static class DataContext
	{
		private static string _folder;
		private static readonly List<BaseStorage> _storages = new List<BaseStorage>();

		public static Action<string> Logger { get; set; }

		public static string Folder => _folder;

		internal static void Log(string message) => Logger?.Invoke(message);

		public static void Register(BaseStorage storage)
		{
			if (_storages.Contains(storage))
			{
				return;
			}

			Log($"Registered data storage for {storage.Name}");
			_storages.Add(storage);
		}

		public static void Unregister(BaseStorage storage)
		{
			if (!_storages.Contains(storage))
			{
				return;
			}

			Log($"Unregistered data storage for {storage.Name}");
			_storages.Remove(storage);
		}

		public static void Load(string folder)
		{
			if (string.IsNullOrEmpty(folder))
			{
				throw new ArgumentNullException(nameof(folder));
			}

			Log($"Loading data from '{folder}'");

			_folder = folder;

			foreach (var storage in _storages)
			{
				storage.Load();
			}

			foreach (var storage in _storages)
			{
				Log($"Setting references for {storage.Name}");
				storage.SetReferences();
			}
		}
	}
}
