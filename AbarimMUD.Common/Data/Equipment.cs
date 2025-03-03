using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum SlotType
	{
		Light,
		RingLeft,
		RingRight,
		Amulet1,
		Amulet2,
		Head,
		Cloak,
		Body,
		Legs,
		Feet,
		Hands,
		Waist,
		WristLeft,
		WristRight,
		Wield,
		Shield,
		Total
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
			Item = item;
		}

		public WearItem Clone() => new WearItem(Slot, Item.Clone());
	}

	public class Equipment
	{
		private static readonly Dictionary<SlotType, ItemType> _slotsArmorsMap = new Dictionary<SlotType, ItemType>();

		public WearItem[] Items { get; set; }

		[JsonIgnore]
		public ItemInstance this[SlotType slot]
		{
			get => Items[(int)slot].Item;
			set => Items[(int)slot].Item = value;
		}

		static Equipment()
		{
			_slotsArmorsMap[SlotType.Light] = ItemType.Light;
			_slotsArmorsMap[SlotType.RingLeft] = ItemType.Ring;
			_slotsArmorsMap[SlotType.RingRight] = ItemType.Ring;
			_slotsArmorsMap[SlotType.Amulet1] = ItemType.Amulet;
			_slotsArmorsMap[SlotType.Amulet2] = ItemType.Amulet;
			_slotsArmorsMap[SlotType.Head] = ItemType.Helmet;
			_slotsArmorsMap[SlotType.Cloak] = ItemType.Cloak;
			_slotsArmorsMap[SlotType.Body] = ItemType.Armor;
			_slotsArmorsMap[SlotType.Legs] = ItemType.Leggings;
			_slotsArmorsMap[SlotType.Feet] = ItemType.Boots;
			_slotsArmorsMap[SlotType.Hands] = ItemType.Gloves;
			_slotsArmorsMap[SlotType.Waist] = ItemType.Belt;
			_slotsArmorsMap[SlotType.WristLeft] = ItemType.Bracelet;
			_slotsArmorsMap[SlotType.WristRight] = ItemType.Bracelet;
			_slotsArmorsMap[SlotType.Wield] = ItemType.Weapon;
			_slotsArmorsMap[SlotType.Shield] = ItemType.Shield;
		}

		public Equipment()
		{
			Items = new WearItem[(int)SlotType.Total];
			for (var i = 0; i < Items.Length; ++i)
			{
				Items[i] = new WearItem((SlotType)i, null);
			}
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
				if (this[slot] == null)
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
			this[freeSlot.Value] = item;

			return true;
		}

		internal ItemInstance Remove(SlotType type)
		{
			var item = this[type];
			if (item == null)
			{
				return null;
			}

			this[type] = null;

			return item;
		}

		public WearItem FindItem(string pat)
		{
			foreach (var wearItem in Items)
			{
				if (wearItem.Item != null && wearItem.Item.MatchesKeyword(pat))
				{
					return wearItem;
				}
			}

			return null;
		}
	}
}
