using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ItemType
	{
		Weapon,
		Armor
	}

	public enum ArmorType
	{
		Ring,
		Amulet,
		Head,
		Body,
		Legs,
		Hands,
		Wrist
	}

	public class Item: IStoredInFile
	{
		public static readonly MultipleFilesStorageString<Item> Storage = new Items();

		public string Id { get; set; }
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public Material Material { get; set; }
		public Affect[] Affects { get; set; } = new Affect[0];
		public ItemType ItemType { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }

		private void EnsureType(ItemType itemType)
		{
		}

		public void SetArmor(ArmorType armorType, int armor)
		{
			ItemType = ItemType.Armor;
			Value1 = (int)armorType;
			Value2 = armor;
		}

		public void GetArmor(out ArmorType armorType, out int armor)
		{
			EnsureType(ItemType.Armor);
			armorType = (ArmorType)Value1;
			armor = Value2;
		}

		public void GetWeapon(out AttackType attackType, out int penetration, out int minimumDamage, out int maximumDamage)
		{
			EnsureType(ItemType.Weapon);
			attackType = (AttackType)Value1;
			penetration = Value2;
			minimumDamage = Value3;
			maximumDamage = Value4;
		}

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Item GetItemById(string id) => Storage.GetByKey(id);
		public static Item EnsureItemById(string id) => Storage.EnsureByKey(id);
	}
}