using System;
using System.Configuration;
using NLog;

namespace AbarimMUD.Utils
{
	public sealed class ConfigValueHolder<T>
	{
		private static readonly Logger _logger = LogUtility.GetGlobalLogger();

		private bool _valueSet;
		private readonly string _key;
		private readonly T _defaultValue;
		private T _value;

		public ConfigValueHolder(string key, T defaultVal = default(T))
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			_key = key;
			_defaultValue = defaultVal;
		}

		public T Value
		{
			get
			{
				if (!_valueSet)
				{
					_logger.Info("Getting Config Setting for '{0}'", _key);
					_value = GetAppSetting(_key, _defaultValue);
					_logger.Info("Value: {0}", _value);
					_valueSet = true;
				}

				return _value;
			}
		}

		public static T GetAppSetting(string key, T defaultVal = default(T))
		{
			T val;
			try
			{
				var reader = new AppSettingsReader();
				val = (T)reader.GetValue(key, typeof(T));
			}
			catch (Exception) // Key or T is null
			{
				val = defaultVal;
			}
			return val;
		}
	}
}