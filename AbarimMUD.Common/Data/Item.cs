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
		Scroll
	}

	public enum ItemFlags
	{
		Stab,
		Artefact
	}

	public enum StockItemType
	{
		Blacksmith,
		Alchemist,
		Scribe,
		Auctioneer
	}

	public class Item : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Item> Storage = new Items();

		private static readonly Dictionary<StockItemType, Item[]> _stockItems = new Dictionary<StockItemType, Item[]>();
		private static bool _stockItemsDirty = true;

		private ItemType _itemType;
		private bool _isStockItem = false;

		public string Id { get; set; }
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public ItemType ItemType
		{
			get => _itemType;

			set
			{
				if (value == _itemType)
				{
					return;
				}

				_itemType = value;
				_stockItemsDirty = true;
			}
		}

		public int Price { get; set; } = 100;
		public AttackType? AttackType { get; set; }
		public Dictionary<ModifierType, Affect> Affects { get; set; } = new Dictionary<ModifierType, Affect>();


		public HashSet<ItemFlags> Flags { get; set; } = new HashSet<ItemFlags>();
		public ValueRange? DamageRange { get; set; }
		public bool IsStockItem
		{
			get => _isStockItem;

			set
			{
				if (value == _isStockItem)
				{
					return;
				}

				_isStockItem = value;
				_stockItemsDirty = true;
			}
		}
		public int? StockItemsCount { get; set; }

		private static void AddItem(Dictionary<StockItemType, List<Item>> dict, StockItemType type, Item item)
		{
			List<Item> list;

			if (!dict.TryGetValue(type, out list))
			{
				list = new List<Item>();
				dict[type] = list;
			}

			list.Add(item);
		}

		private static void UpdateStockItems()
		{
			if (!_stockItemsDirty)
			{
				return;
			}

			var temp = new Dictionary<StockItemType, List<Item>>();
			foreach (var item in Storage)
			{
				if (!item.IsStockItem)
				{
					continue;
				}

				AddItem(temp, item.ItemType.GetStockItemType(), item);
			}

			_stockItems.Clear();

			foreach (StockItemType type in Enum.GetValues(typeof(StockItemType)))
			{
				List<Item> items;
				if (temp.TryGetValue(type, out items))
				{
					_stockItems[type] = items.ToArray();
				}
				else
				{
					_stockItems[type] = new Item[0];
				}
			}

			_stockItemsDirty = false;
		}

		public static Item[] GetStockItems(StockItemType type)
		{
			UpdateStockItems();

			return _stockItems[type];
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
				ItemType = ItemType,
				Affects = Affects,
				DamageRange = DamageRange,
				IsStockItem = IsStockItem,
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
		private static readonly Dictionary<ItemType, StockItemType> _itemsStockTypes = new Dictionary<ItemType, StockItemType>();

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

			_itemsStockTypes[ItemType.Ring] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Amulet] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Helmet] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Armor] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Bracelet] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Gloves] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Leggings] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Boots] = StockItemType.Blacksmith;
			_itemsStockTypes[ItemType.Weapon] = StockItemType.Blacksmith;

			_itemsStockTypes[ItemType.Potion] = StockItemType.Alchemist;

			_itemsStockTypes[ItemType.Scroll] = StockItemType.Scribe;
		}

		public static bool IsArmor(this ItemType type) => _armorTypes[(int)type];

		public static StockItemType GetStockItemType(this ItemType type) => _itemsStockTypes[type];
	}
}