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
		public List<InventoryRecord> Items { get; set; } = new List<InventoryRecord>();

		public void AddItem(ItemInstance item, int quantity)
		{
			if (quantity == 0)
			{
				return;
			}

			var existingItem = (from i in Items where ItemInstance.AreEqual(i.Item, item) select i).FirstOrDefault();
			if (existingItem != null)
			{
				existingItem.Quantity += quantity;
				if (existingItem.Quantity == 0)
				{
					Items.Remove(existingItem);
				}
			}
			else
			{
				if (quantity < 0)
				{
					throw new ArgumentOutOfRangeException($"Can't add new item with negative quantity");
				}

				var rec = new InventoryRecord(item, quantity);
				Items.Add(rec);
			}
		}

		public void AddItem(InventoryRecord rec) => AddItem(rec.Item, rec.Quantity);

		public void AddInventory(Inventory other)
		{
			foreach(var rec in other.Items)
			{
				AddItem(rec);
			}
		}

		public InventoryRecord FindItem(string keyword)
		{
			return (from i in Items where i.Item.MatchesKeyword(keyword) select i).FirstOrDefault();
		}

		public void Clear() => Items.Clear();

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
