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


		public void GetArmor(out ArmorType armorType, out int armor) => Info.GetArmor(out armorType, out armor);
		public void GetWeapon(out AttackType attackType, out int penetration, out int minimumDamage, out int maximumDamage) =>
			Info.GetWeapon(out attackType, out penetration, out minimumDamage, out maximumDamage);

		public static bool AreEqual(ItemInstance a, ItemInstance b) => a.Id == b.Id;
	}
}
