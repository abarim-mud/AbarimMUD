using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public enum SlotType
	{
		Light,
		Finger,
		Neck,
		Head,
		Cloak,
		Body,
		Legs,
		Feet,
		Hands,
		Waist,
		Wrist,
		Wield,
		Shield
	}

	public class WearItem
	{
		public SlotType Slot { get; private set; }
		public int Index { get; private set; }
		public ItemInstance Item { get; set; }

		public WearItem(SlotType slot, int index = 0)
		{
			Slot = slot;
			Index = index;
		}
	}

	public class Equipment
	{
		public WearItem[] Items { get; private set; }

		public Equipment()
		{
			var items = new List<WearItem>
			{
				new WearItem(SlotType.Light),
				new WearItem(SlotType.Finger),
				new WearItem(SlotType.Finger, 1),
				new WearItem(SlotType.Neck),
				new WearItem(SlotType.Neck, 1),
				new WearItem(SlotType.Head),
				new WearItem(SlotType.Cloak),
				new WearItem(SlotType.Body),
				new WearItem(SlotType.Legs),
				new WearItem(SlotType.Feet),
				new WearItem(SlotType.Hands),
				new WearItem(SlotType.Waist),
				new WearItem(SlotType.Wrist),
				new WearItem(SlotType.Wrist, 1),
				new WearItem(SlotType.Wield),
				new WearItem(SlotType.Shield)
			};

			Items = items.ToArray();
		}

		public WearItem GetSlot(SlotType slot, int index = 0)
		{
			return (from s in Items where s.Slot == slot && s.Index == index select s).FirstOrDefault();
		}

		internal bool? Wear(ItemInstance item)
		{
			if (item.Info.EquipmentSlot == null)
			{
				// Item can't be worn at all
				return null;
			}

			var slot = item.Info.EquipmentSlot.Value;

			WearItem freeSlot = null;
			for (var i = 0; i < Items.Length; ++i)
			{
				var s = Items[i];
				if (s.Slot == slot && s.Item == null)
				{
					freeSlot = s;
					break;
				}
			}

			if (freeSlot == null)
			{
				// No free slots
				return false;
			}

			// Wear the item
			freeSlot.Item = item;

			return true;
		}

		internal ItemInstance Remove(SlotType slot)
		{
			WearItem occupiedSlot = null;
			for (var i = 0; i < Items.Length; ++i)
			{
				var s = Items[i];
				if (s.Slot == slot && s.Item != null)
				{
					occupiedSlot = s;
					break;
				}
			}

			if (occupiedSlot == null)
			{
				return null;
			}

			var result = occupiedSlot.Item;
			occupiedSlot.Item = null;

			return result;
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
