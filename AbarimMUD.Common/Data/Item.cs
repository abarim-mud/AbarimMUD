using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ItemType
	{
		Ring,
		Amulet,
		Helmet,
		Armor,
		Bracelet,
		Gloves,
		Leggings,
		Boots,
		Weapon,
	}

	public enum ItemFlags
	{
		Stab
	}

	public class Item : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Item> Storage = new Items();

		private int _value1, _value2, _value3, _value4;

		public string Id { get; set; }
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public Affect[] Affects { get; set; } = new Affect[0];
		public ItemType ItemType { get; set; }
		public AttackType AttackType { get; set; }

		public HashSet<ItemFlags> Flags { get; set; } = new HashSet<ItemFlags>();

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
				InvalidateCreaturesWithThisItem();
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
				InvalidateCreaturesWithThisItem();
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
				InvalidateCreaturesWithThisItem();
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
				InvalidateCreaturesWithThisItem();
			}
		}

		private void EnsureType(ItemType itemType)
		{
			if (ItemType != itemType)
			{
				throw new Exception($"Item {Id} of type {ItemType} isn't {itemType}");
			}
		}

		public void SetArmor(int armor)
		{
			Value1 = armor;
		}

		public void GetArmor(out int armor)
		{
			if (!ItemType.IsArmor())
			{
				throw new Exception($"Item {Id} of type {ItemType} isn't armor.");
			}

			armor = Value1;
		}

		public void SetWeapon(int penetration, int minimumDamage, int maximumDamage)
		{
			ItemType = ItemType.Weapon;
			Value1 = penetration;
			Value2 = minimumDamage;
			Value3 = maximumDamage;
		}

		public void GetWeapon(out int penetration, out int minimumDamage, out int maximumDamage)
		{
			EnsureType(ItemType.Weapon);
			penetration = Value1;
			minimumDamage = Value2;
			maximumDamage = Value3;
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
				AttackType = AttackType,
				Affects = Affects,
				ItemType = ItemType,
				_value1 = _value1,
				_value2 = _value2,
				_value3 = _value3,
				_value4 = _value4,
			};

			foreach (var keyword in Keywords)
			{
				result.Keywords.Add(keyword);
			}

			return result;
		}

		public object Clone() => CloneItem();

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		private void InvalidateCreaturesWithThisItem()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				foreach (var item in creature.Equipment.Items)
				{
					if (item.Item.Info.Id == Id)
					{
						creature.InvalidateStats();
						break;
					}
				}
			}
		}

		public static Item GetItemById(string id) => Storage.GetByKey(id);
		public static Item EnsureItemById(string id) => Storage.EnsureByKey(id);
	}

	public static class ItemExtensions
	{
		private static readonly bool[] _armorTypes;

		static ItemExtensions()
		{
			_armorTypes = new bool[Enum.GetValues(typeof(ItemType)).Length];

			Array.Fill(_armorTypes, false);

			_armorTypes[(int)ItemType.Ring] = true;
			_armorTypes[(int)ItemType.Amulet] = true;
			_armorTypes[(int)ItemType.Helmet] = true;
			_armorTypes[(int)ItemType.Armor] = true;
			_armorTypes[(int)ItemType.Bracelet] = true;
			_armorTypes[(int)ItemType.Gloves] = true;
			_armorTypes[(int)ItemType.Leggings] = true;
			_armorTypes[(int)ItemType.Boots] = true;
		}

		public static bool IsArmor(this ItemType type) => _armorTypes[(int)type];
	}
}