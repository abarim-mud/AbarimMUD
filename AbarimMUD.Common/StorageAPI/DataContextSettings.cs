using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;

namespace AbarimMUD.Storage
{
	internal class DataContextSettings
	{
		private readonly Action<string> _log;

		public string Folder { get; private set; }

		public DataContextSettings(string folder, Action<string> log)
		{
			Folder = folder;
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
