namespace AbarimMUD.Common.Data
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

	public class AreaReset: Entity
	{
		public Area Area { get; set; }
		public int AreaId { get; set; }

		public AreaResetType ResetType { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }
		public int Value5 { get; set; }
	}
}
