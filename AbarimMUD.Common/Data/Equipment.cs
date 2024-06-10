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
		public Item Item { get; set; }

		public WearItem()
		{
		}

		public WearItem(SlotType slot, Item item)
		{
			Slot = slot;
			Item = item ?? throw new ArgumentNullException(nameof(item));
		}
	}

	public class Equipment
	{
		private static readonly Dictionary<SlotType, ArmorType> _slotsArmorsMap = new Dictionary<SlotType, ArmorType>();
		private readonly Dictionary<SlotType, Item> _items = new Dictionary<SlotType, Item>();

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

		static Equipment()
		{
			_slotsArmorsMap[SlotType.FingerLeft] = ArmorType.Ring;
			_slotsArmorsMap[SlotType.FingerRight] = ArmorType.Ring;
			_slotsArmorsMap[SlotType.Neck] = ArmorType.Amulet;
			_slotsArmorsMap[SlotType.Body] = ArmorType.Body;
			_slotsArmorsMap[SlotType.Head] = ArmorType.Head;
			_slotsArmorsMap[SlotType.Legs] = ArmorType.Legs;
			_slotsArmorsMap[SlotType.WristLeft] = ArmorType.Wrist;
			_slotsArmorsMap[SlotType.WristRight] = ArmorType.Wrist;
		}

		internal bool? Wear(Item item)
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
	}
}
