using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public static class Configuration
	{
		private static readonly CustomStorage<ConfigurationInstance> InternalStorage = new CustomStorage<ConfigurationInstance>("settings.json");


		private class ConfigurationInstance
		{
			public int MaximumLevel { get; set; }
			public int ServerPort { get; set; }
			public string WebServiceUrl { get; set; }
			public string SplashFile { get; set; }
			public int StartRoomId { get; set; }
			public string DefaultCharacter { get; set; }
			public string DefaultClass { get; set; }

			public int PauseBetweenFightRoundsInMs { get; set; }
			public int NegativeRegen { get; set; }
		}

		public static BaseStorage Storage => InternalStorage;

		private static ConfigurationInstance Instance => InternalStorage.Item;

		public static int MaximumLevel => Instance.MaximumLevel;
		public static int ServerPort => Instance.ServerPort;
		public static string WebServiceUrl => Instance.WebServiceUrl;
		public static string SplashFile => Instance.SplashFile;
		public static int StartRoomId => Instance.StartRoomId;
		public static string DefaultCharacter => Instance.DefaultCharacter;
		public static string DefaultClass => Instance.DefaultClass;
		public static int PauseBetweenFightRoundsInMs => Instance.PauseBetweenFightRoundsInMs;

		/// <summary>
		/// Negative regen rate(per minute), used when values(hp/mana/mv) are higher than their maxes
		/// </summary>
		public static int NegativeRegen => Instance.NegativeRegen;

		public static void Save() => InternalStorage.Save();
	}
}
