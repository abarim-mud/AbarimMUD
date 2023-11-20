using AbarimMUD.Utils;

namespace AbarimMUD
{
	public static class Configuration
	{
		private static readonly ConfigValueHolder<string> _mongoPath = new ConfigValueHolder<string>("MongoPath",
			"mongodb://localhost");

		private static readonly ConfigValueHolder<string> _database = new ConfigValueHolder<string>("Database", "AbarimMUD");
		private static readonly ConfigValueHolder<string> _adminName = new ConfigValueHolder<string>("AdminName", "admin");

		private static readonly ConfigValueHolder<string> _adminPassword = new ConfigValueHolder<string>("AdminPassword",
			"admin");

		private static readonly ConfigValueHolder<string> _adminCharacter = new ConfigValueHolder<string>("AdminCharacter",
			"abarim");

		private static readonly ConfigValueHolder<int> _serverPort = new ConfigValueHolder<int>("Port", 6101);

		private static readonly ConfigValueHolder<string> _splashFile = new ConfigValueHolder<string>("SplashFile",
			string.Empty);

		private static readonly ConfigValueHolder<string> _webServiceUrl = new ConfigValueHolder<string>("WebServiceUrl", "http://localhost:8080/AbarimMUD");

		public static string MongoPath
		{
			get { return _mongoPath.Value; }
		}

		public static string Database
		{
			get { return _database.Value; }
		}

		public static string AdminName
		{
			get { return _adminName.Value; }
		}

		public static string AdminPassword
		{
			get { return _adminPassword.Value; }
		}

		public static string AdminCharacter
		{
			get { return _adminCharacter.Value; }
		}

		public static int ServerPort
		{
			get { return _serverPort.Value; }
		}

		public static string SplashFile
		{
			get { return _splashFile.Value; }
		}

		public static string WebServiceUrl
		{
			get { return _webServiceUrl.Value; }
		}
	}
}