namespace AbarimMUD.Data
{
	public enum AreaResetType
	{
		Mobile,
		GameObject,
		Put,
		Give,
		Equip,
		Door,
		Randomize
	}

	public class AreaReset: AreaEntity
	{
		public AreaResetType ResetType { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }
		public int Value5 { get; set; }
	}
}
