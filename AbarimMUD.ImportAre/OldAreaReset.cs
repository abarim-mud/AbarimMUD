using AbarimMUD.Data;

namespace AbarimMUD.ImportAre
{
	internal class OldAreaReset
	{
		public AreaResetType ResetType { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }
		public int Value5 { get; set; }

		public AreaReset ToNewAreaReset()
		{
			var result = new AreaReset
			{
				ResetType = ResetType,
				Id1 = Value4,
				Count = Value3,
				Max = Value5,
				Id2 = Value2,
			};

			return result;
		}
	}
}
