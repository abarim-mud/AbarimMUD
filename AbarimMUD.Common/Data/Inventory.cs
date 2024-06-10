using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Data
{
	public class Inventory
	{
		private readonly List<ItemInstance> _items = new List<ItemInstance>();
		private ItemInstance[] _itemsArray = null;

		public ItemInstance[] Items
		{
			get
			{
				if (_itemsArray == null)
				{
					_itemsArray = _items.ToArray();
				}

				return _itemsArray;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_itemsArray = value;

				_items.Clear();
				_items.AddRange(value);
			}
		}

		public void AddItem(ItemInstance item)
		{
			if (item.Quantity == 0)
			{
				return;
			}

			var existingItem = (from i in _items where ItemInstance.AreEqual(i, item) select i).FirstOrDefault();
			if (existingItem != null)
			{
				existingItem.Quantity += item.Quantity;
				if (existingItem.Quantity == 0)
				{
					_items.Remove(existingItem);
				}
			}
			else
			{
				_items.Add(item);
			}

			InvalidateArray();
		}

		private void InvalidateArray()
		{
			_itemsArray = null;
		}

	}
}
