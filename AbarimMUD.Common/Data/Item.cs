using AbarimMUD.Storage;
using System.Text.Json.Serialization;

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

	public enum WeaponType
	{
		Exotic,
		Sword,
		Mace,
		Dagger,
		Axe,
		Staff,
		Flail,
		Whip,
		Polearm
	}

	public class Item
	{
		public static readonly MultipleFilesStorageString<Item> Storage = new MultipleFilesStorageString<Item>(r => r.Name, "items");
		private static int _id = 1;

		[JsonIgnore]
		public int Id { get; set; }
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string Description { get; set; }
		public Material Material { get; set; }
		public Affect[] Affects { get; set; } = new Affect[0];
		public ItemType ItemType { get; set; }
		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }

		public Item()
		{
			Id = _id;
			++_id;
		}

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

		public void SetWeapon(WeaponType weaponType, int penetration, int minimumDamage, int maximumDamage)
		{
			ItemType = ItemType.Weapon;
			Value1 = (int)weaponType;
			Value2 = penetration;
			Value3 = minimumDamage;
			Value4 = maximumDamage;
		}

		public void GetWeapon(out WeaponType weaponType, out int penetration, out int minimumDamage, out int maximumDamage)
		{
			EnsureType(ItemType.Weapon);
			weaponType = (WeaponType)Value1;
			penetration = Value2;
			minimumDamage = Value3;
			maximumDamage = Value4;
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public override string ToString() => $"{Name} (#{Id})";

		public static Item GetItemByName(string name) => Storage.GetByKey(name);
		public static Item EnsureItemByName(string name) => Storage.EnsureByKey(name);
		public static Item LookupItem(string name) => Storage.Lookup(name);
	}
}