namespace AbarimMUD
{
	public static class Configuration
	{
		public static int ServerPort => 6101;
		public static string WebServiceUrl => "http://localhost:8080/AbarimMUD/";
		public static string SplashFile => string.Empty;
		public static string StartAreaName = "Midgaard";
		public static int StartRoomId => 0;
		public static string DataFolder;
	}
}
