using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class ItemInstance
	{
		public Item Info { get; set; }

		public string Id => Info.Id;

		[JsonIgnore]
		public string ShortDescription => Info.ShortDescription;

		[JsonIgnore]
		public ItemType ItemType => Info.ItemType;

		public ItemInstance()
		{
		}

		public ItemInstance(Item item)
		{
			Info = item;
		}


		public bool MatchesKeyword(string keyword) => Info.MatchesKeyword(keyword);


		public void GetArmor(out int armor) => Info.GetArmor(out armor);
		public void GetWeapon(out int penetration, out int minimumDamage, out int maximumDamage) =>
			Info.GetWeapon(out penetration, out minimumDamage, out maximumDamage);

		public override string ToString() => Info.ToString();

		public static bool AreEqual(ItemInstance a, ItemInstance b) => a.Id == b.Id;
	}
}
