using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public enum SlotType
	{
		Light,
		FingerLeft,
		FingerRight,
		Neck,
		Body,
		Head,
		Legs,
		Feet,
		Hands,
		Arms,
		Shield,
		About,
		Waist,
		WristLeft,
		WristRight,
		Wield,
		Hold,
		Float
	}

	public class WearItem
	{
		public SlotType Slot { get; set; }
		public ItemInstance Item { get; set; }

		public WearItem()
		{
		}

		public WearItem(SlotType slot, ItemInstance item)
		{
			Slot = slot;
			Item = item ?? throw new ArgumentNullException(nameof(item));
		}

		public WearItem Clone() => new WearItem(Slot, Item.Clone());
	}

	public class Equipment
	{
		private static readonly Dictionary<SlotType, ItemType> _slotsArmorsMap = new Dictionary<SlotType, ItemType>();
		private readonly SortedDictionary<SlotType, ItemInstance> _items = new SortedDictionary<SlotType, ItemInstance>();

		public WearItem[] Items
		{
			get => (from pair in _items select new WearItem(pair.Key, pair.Value)).ToArray();

			set
			{
				_items.Clear();

				foreach (var item in value)
				{
					_items[item.Slot] = item.Item;
				}
			}
		}

		public ItemInstance this[SlotType slot]
		{
			get
			{
				ItemInstance item;
				if (!_items.TryGetValue(slot, out item))
				{
					return null;
				}

				return item;
			}
		}

		static Equipment()
		{
			_slotsArmorsMap[SlotType.FingerLeft] = ItemType.Ring;
			_slotsArmorsMap[SlotType.FingerRight] = ItemType.Ring;
			_slotsArmorsMap[SlotType.Neck] = ItemType.Amulet;
			_slotsArmorsMap[SlotType.Head] = ItemType.Helmet;
			_slotsArmorsMap[SlotType.Body] = ItemType.Armor;
			_slotsArmorsMap[SlotType.WristLeft] = ItemType.Bracelet;
			_slotsArmorsMap[SlotType.WristRight] = ItemType.Bracelet;
			_slotsArmorsMap[SlotType.Hands] = ItemType.Gloves;
			_slotsArmorsMap[SlotType.Legs] = ItemType.Leggings;
			_slotsArmorsMap[SlotType.Feet] = ItemType.Boots;
			_slotsArmorsMap[SlotType.Wield] = ItemType.Weapon;
		}

		internal bool? Wear(ItemInstance item)
		{
			// Get all possible free slots
			var possibleSlots = (from s in _slotsArmorsMap where s.Value == item.ItemType select s.Key).ToArray();
			if (possibleSlots.Length == 0)
			{
				// Item can't be worn at all
				return null;
			}

			SlotType? freeSlot = null;
			foreach (var slot in possibleSlots)
			{
				if (!_items.ContainsKey(slot))
				{
					freeSlot = slot;
					break;
				}
			}

			if (freeSlot == null)
			{
				// No free slots
				return false;
			}

			// Wear the item
			_items[freeSlot.Value] = item;

			return true;
		}

		internal ItemInstance Remove(SlotType type)
		{
			ItemInstance item;
			if (!_items.TryGetValue(type, out item))
			{
				return null;
			}

			_items.Remove(type);

			return item;
		}

		public WearItem FindItem(string pat)
		{
			foreach (var pair in _items)
			{
				if (pair.Value.MatchesKeyword(pat))
				{
					return new WearItem(pair.Key, pair.Value);
				}
			}

			return null;
		}
	}
}
