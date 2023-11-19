namespace AbarimMUD.Common.Data
{
	public class Shop: AreaEntity
	{
		public int MobileId { get; set; }
		public Mobile Mobile { get; set; }

		public int BuyType1 { get; set; }
		public int BuyType2 { get; set; }
		public int BuyType3 { get; set; }
		public int BuyType4 { get; set; }
		public int BuyType5 { get; set; }
		public int ProfitBuy { get; set; }
		public int ProfitSell { get; set;}
		public int OpenHour { get; set; }
		public int CloseHour { get; set; }

	}
}
