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
	}

	public class Equipment
	{
		private static readonly Dictionary<SlotType, ArmorType> _slotsArmorsMap = new Dictionary<SlotType, ArmorType>();
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
			_slotsArmorsMap[SlotType.FingerLeft] = ArmorType.Ring;
			_slotsArmorsMap[SlotType.FingerRight] = ArmorType.Ring;
			_slotsArmorsMap[SlotType.Neck] = ArmorType.Amulet;
			_slotsArmorsMap[SlotType.Head] = ArmorType.Head;
			_slotsArmorsMap[SlotType.Body] = ArmorType.Body;
			_slotsArmorsMap[SlotType.Hands] = ArmorType.Hands;
			_slotsArmorsMap[SlotType.Legs] = ArmorType.Legs;
			_slotsArmorsMap[SlotType.WristLeft] = ArmorType.Wrist;
			_slotsArmorsMap[SlotType.WristRight] = ArmorType.Wrist;
		}

		internal bool? Wear(ItemInstance item)
		{
			if (item.ItemType == ItemType.Armor)
			{
				ArmorType armorType;
				int armor;

				item.GetArmor(out armorType, out armor);

				// Get all possible free slots
				var possibleSlots = (from s in _slotsArmorsMap where s.Value == armorType select s.Key).ToArray();

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
					return false;
				}

				_items[freeSlot.Value] = item;

				return true;
			}
			else if (item.ItemType == ItemType.Weapon)
			{
				if (_items.ContainsKey(SlotType.Wield))
				{
					return false;
				}

				_items[SlotType.Wield] = item;
				return true;
			}

			return null;
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
