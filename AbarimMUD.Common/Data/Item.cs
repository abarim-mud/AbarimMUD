using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System;
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

	public class Item : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorageString<Item> Storage = new Items();

		private int _value1, _value2, _value3, _value4;

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

		[OLCIgnore]
		public int Value1
		{
			get => _value1;

			set
			{
				if (value == _value1)
				{
					return;
				}

				_value1 = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		[OLCIgnore]
		public int Value2
		{
			get => _value2;

			set
			{
				if (value == _value2)
				{
					return;
				}

				_value2 = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		[OLCIgnore]
		public int Value3
		{
			get => _value3;

			set
			{
				if (value == _value3)
				{
					return;
				}

				_value3 = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		[OLCIgnore]
		public int Value4
		{
			get => _value4;

			set
			{
				if (value == _value4)
				{
					return;
				}

				_value4 = value;
				Creature.InvalidateAllCreaturesStats();
			}
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

		public void SetWeapon(AttackType attackType, int penetration, int minimumDamage, int maximumDamage)
		{
			ItemType = ItemType.Weapon;
			Value1 = (int)attackType;
			Value2 = penetration;
			Value3 = minimumDamage;
			Value4 = maximumDamage;
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

		public Item CloneItem()
		{
			var result = new Item
			{
				Id = Id,
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Material = Material,
				Affects = Affects,
				ItemType = ItemType,
				_value1 = _value1,
				_value2 = _value2,
				_value3 = _value3,
				_value4 = _value4,
			};

			foreach(var keyword in Keywords)
			{
				result.Keywords.Add(keyword);
			}

			return result;
		}

		public object Clone() => CloneItem();

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Item GetItemById(string id) => Storage.GetByKey(id);
		public static Item EnsureItemById(string id) => Storage.EnsureByKey(id);
	}
}