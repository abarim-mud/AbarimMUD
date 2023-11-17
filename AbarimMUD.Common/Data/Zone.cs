namespace AbarimMUD.Common.Data
{
	public enum ResetMode
	{
		None,
		ResetIfNoPC,
		ResetAlways
	}

	public class Zone
	{
		public int Id { get; set; }
		public int? VNum { get; set; }
		public string Builder { get; set; }
		public string Name { get; set; }
		public int StartRoomVNum { get; set; }
		public int MaximumRooms { get; set; }
		public int ResetInMinutes { get; set; }
		public ResetMode ResetMode { get; set; }
		public int? MinimumLevel { get; set; }
		public int? MaximumLevel { get; set; }
		public long Flags1 { get; set; }
		public long Flags2 { get; set; }
		public long Flags3 { get; set; }
		public long Flags4 { get; set; }
	}
}
