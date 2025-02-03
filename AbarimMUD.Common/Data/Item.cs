using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.Utils;
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
		Potion,
	}

	public enum ItemFlags
	{
		Stab,
		Artefact
	}

	public class Item : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Item> Storage = new Items();

		public string Id { get; set; }
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public ItemType ItemType { get; set; }
		public AttackType AttackType { get; set; }
		public Dictionary<ModifierType, Affect> Affects { get; set; } = new Dictionary<ModifierType, Affect>();


		public HashSet<ItemFlags> Flags { get; set; } = new HashSet<ItemFlags>();
		public RandomRange? DamageRange { get; set; }

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
				ItemType = ItemType,
				Affects = Affects,
				DamageRange = DamageRange,
			};

			foreach (var flag in Flags)
			{
				result.Flags.Add(flag);
			}

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