using System.Collections.Generic;

namespace AbarimMUD.Common.Data
{
	public class GameObject : AreaEntity
	{
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string Description { get; set; }
		public Material Material { get; set; }
		public ItemType ItemType { get; set; }
		public ItemWearFlags WearFlags { get; set; }
		public ItemExtraFlags ExtraFlags { get; set; }

		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }
		public int Value5 { get; set; }
		public int Level { get; set; }
		public int Weight { get; set; }
		public int Cost { get; set; }
		public int Condition { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }

		public ICollection<GameObjectEffect> Effects { get; }
	}
}
