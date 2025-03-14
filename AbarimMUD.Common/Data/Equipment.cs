using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public class EquipmentSlot
	{
		public EquipmentSlotType SlotType { get; private set; }
		public int Index { get; private set; }
		public ItemInstance Item { get; set; }

		public EquipmentSlot(EquipmentSlotType slotType, int index = 0)
		{
			SlotType = slotType;
			Index = index;
		}
	}

	public class Equipment
	{
		public EquipmentSlot[] Slots { get; private set; }

		public Equipment()
		{
			var slots = new List<EquipmentSlot>
			{
				new EquipmentSlot(EquipmentSlotType.Light),
				new EquipmentSlot(EquipmentSlotType.Finger),
				new EquipmentSlot(EquipmentSlotType.Finger, 1),
				new EquipmentSlot(EquipmentSlotType.Neck),
				new EquipmentSlot(EquipmentSlotType.Neck, 1),
				new EquipmentSlot(EquipmentSlotType.Head),
				new EquipmentSlot(EquipmentSlotType.Cloak),
				new EquipmentSlot(EquipmentSlotType.Body),
				new EquipmentSlot(EquipmentSlotType.Legs),
				new EquipmentSlot(EquipmentSlotType.Feet),
				new EquipmentSlot(EquipmentSlotType.Hands),
				new EquipmentSlot(EquipmentSlotType.Waist),
				new EquipmentSlot(EquipmentSlotType.Wrist),
				new EquipmentSlot(EquipmentSlotType.Wrist, 1),
				new EquipmentSlot(EquipmentSlotType.Wield),
				new EquipmentSlot(EquipmentSlotType.Shield)
			};

			Slots = slots.ToArray();
		}

		public EquipmentSlot GetSlot(EquipmentSlotType slot, int index = 0)
		{
			return (from s in Slots where s.SlotType == slot && s.Index == index select s).FirstOrDefault();
		}

		internal bool? Wear(ItemInstance item)
		{
			if (item.Info.EquipmentSlot == null)
			{
				// Item can't be worn at all
				return null;
			}

			var slot = item.Info.EquipmentSlot.Value;

			EquipmentSlot freeSlot = null;
			for (var i = 0; i < Slots.Length; ++i)
			{
				var s = Slots[i];
				if (s.SlotType == slot && s.Item == null)
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

		internal ItemInstance Remove(EquipmentSlotType slot)
		{
			EquipmentSlot occupiedSlot = null;
			for (var i = 0; i < Slots.Length; ++i)
			{
				var s = Slots[i];
				if (s.SlotType == slot && s.Item != null)
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

		public EquipmentSlot Find(string pat)
		{
			foreach (var wearItem in Slots)
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
