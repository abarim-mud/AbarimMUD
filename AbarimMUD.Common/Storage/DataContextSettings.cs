using System;

namespace AbarimMUD.Storage
{
	internal class DataContextSettings
	{
		private readonly string _folder;
		private readonly Action<string> _log;

		public string Folder => _folder;

		public DataContextSettings(string folder, Action<string> log)
		{
			_folder = folder;
			_log = log;
		}

		public void Log(string message)
		{
			if (_log == null)
			{
				return;
			}

			_log(message);
		}
	}
}
