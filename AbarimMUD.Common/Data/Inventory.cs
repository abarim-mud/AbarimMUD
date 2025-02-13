using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class InventoryRecord
	{
		public ItemInstance Item { get; set; }
		public int Quantity { get; set; } = 1;

		[JsonIgnore]
		public string Id => Info.Id;

		[JsonIgnore]
		public string ShortDescription => Info.ShortDescription;

		[JsonIgnore]
		public Item Info
		{
			get => Item.Info;
			set => Item.Info = value;
		}

		public InventoryRecord()
		{
		}

		public InventoryRecord(ItemInstance item, int quantity)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item));
			Quantity = quantity;
		}

		public InventoryRecord Clone() => new InventoryRecord(Item.Clone(), Quantity);

		public override string ToString()
		{
			var result = Item.ToString();

			if (Quantity > 1)
			{
				result += $" ({Quantity})";
			}

			return result;
		}
	}

	public class Inventory
	{
		private readonly List<InventoryRecord> _items = new List<InventoryRecord>();
		private InventoryRecord[] _itemsArray = null;

		public InventoryRecord[] Items
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

		public void AddItem(ItemInstance item, int quantity)
		{
			if (quantity == 0)
			{
				return;
			}

			var existingItem = (from i in _items where ItemInstance.AreEqual(i.Item, item) select i).FirstOrDefault();
			if (existingItem != null)
			{
				existingItem.Quantity += quantity;
				if (existingItem.Quantity == 0)
				{
					_items.Remove(existingItem);
				}
			}
			else
			{
				if (quantity < 0)
				{
					throw new ArgumentOutOfRangeException($"Can't add new item with negative quantity");
				}

				var rec = new InventoryRecord(item, quantity);
				_items.Add(rec);
			}

			InvalidateArray();
		}

		public void AddItem(InventoryRecord rec) => AddItem(rec.Item, rec.Quantity);

		private void InvalidateArray()
		{
			_itemsArray = null;
		}

		public InventoryRecord FindItem(string keyword)
		{
			return (from i in Items where i.Item.MatchesKeyword(keyword) select i).FirstOrDefault();
		}

		public Inventory Clone()
		{
			var result = new Inventory();

			foreach (var rec in Items)
			{
				result.AddItem(rec.Item, rec.Quantity);
			}

			return result;
		}
	}
}
