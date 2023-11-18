namespace AbarimMUD.Common.Data
{
	public class Mobile: AreaEntity
	{
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public Race Race { get; set; }
		public int Flags { get; set; }
		public int AffectedBy { get; set; }
		public int Alignment { get; set; }
		public int Group { get; set; }
		public int Level { get; set; }
		public int HitRoll { get; set; }
		public string HitDice { get; set; }
		public string ManaDice { get; set; }
		public string DamageDice { get; set; }
		public AttackType AttackType { get; set; }
		public int AcPierce { get; set; }
		public int AcBash { get; set; }
		public int AcSlash { get; set; }
		public int AcExotic { get; set; }
		public int OffenseFlags { get; set; }
		public int ImmuneFlags { get; set; }
		public int ResistanceFlags { get; set; }
		public int VulnerableFlags { get; set; }
		public MobilePosition StartPosition { get; set; }
		public MobilePosition DefaultPosition { get; set; }
		public Sex Sex { get; set; }
		public int Wealth { get; set; }
		public int FormsFlags { get; set; }
		public int PartsFlags { get; set; }
		public MobileSize Size { get; set; }
		public Material Material { get; set; }
	}
}
