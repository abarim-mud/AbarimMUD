using AbarimMUD.Data;

namespace AbarimMUD.Site
{
	public static class Database
	{
		public static string ConnectionString { get; set; }

		public static DataContext CreateDataContext() => new DataContext(ConnectionString);
	}
}
