namespace AbarimMUD.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Area : Entity
	{
		public string Name { get; set; }
		public string Credits { get; set; }
		public string Builders { get; set; }
		public int? MinimumLevel { get; set; }
		public int? MaximumLevel { get; set; }
	}
}
