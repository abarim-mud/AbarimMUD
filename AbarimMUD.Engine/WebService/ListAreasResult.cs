namespace AbarimMUD.WebService
{
	public class ListAreasResult
	{
		public class AreaInfo
		{
			public string Id { get; set; }

			public string Name { get; set; }
		}

		public ResultDescription Result { get; set; }

		public AreaInfo[] Areas { get; set; }
	}
}