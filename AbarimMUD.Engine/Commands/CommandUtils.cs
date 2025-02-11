using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AbarimMUD.Commands
{
	public static class CommandUtils
	{
		public static bool EnsureInt(this ExecutionContext context, string value, out int result)
		{
			if (!int.TryParse(value, out result))
			{
				context.Send($"Unable to parse number '{value}'");
				return false;
			}

			return true;
		}

		public static bool EnsureLong(this ExecutionContext context, string value, out long result)
		{
			if (!value.TryParseBigNumber(out result))
			{
				context.Send($"Unable to parse number '{value}'");
				return false;
			}

			return true;
		}

		public static bool EnsureBool(this ExecutionContext context, string value, out bool result)
		{
			if (!bool.TryParse(value, out result))
			{
				context.Send($"Unable to parse boolean value '{value}'");
				return false;
			}

			return true;
		}

		public static bool EnsureEnum(this ExecutionContext context, string value, Type enumType, out object result)
		{
			if (!Enum.TryParse(enumType, value, true, out result))
			{
				context.Send($"Unable to parse enum '{value}' of type {enumType.Name}");
				return false;
			}

			return true;
		}

		public static bool EnsureEnum<T>(this ExecutionContext context, string value, out T result) where T : struct
		{
			if (!Enum.TryParse(value, true, out result))
			{
				context.Send($"Unable to parse enum '{value}' of type {typeof(T).Name}");
				return false;
			}

			return true;
		}

		public static IOLCStorage EnsureStorage(this ExecutionContext context, string key)
		{
			var result = OLCManager.GetStorage(key);
			if (result == null)
			{
				context.Send($"Unknown entity '{key}'.");
			}

			return result;
		}

		public static object EnsureItemById(this ExecutionContext context, IOLCStorage storage, string id)
		{
			var item = storage.FindById(context, id);
			if (item == null)
			{
				var objectType = storage.ObjectType.Name.ToLower();
				context.Send($"Unable to find item of type {objectType} by id '{id}'");
				return null;
			}

			return item;
		}

		public static InventoryRecord EnsureItemInInventory(this ExecutionContext context, string name)
		{
			var item = context.Creature.Inventory.FindItem(name);
			if (item == null)
			{
				context.Send($"You don't have an item '{name}'");
			}

			return item;
		}

		public static WearItem EnsureItemWorn(this ExecutionContext context, string name)
		{
			var item = context.Creature.Equipment.FindItem(name);
			if (item == null)
			{
				context.Send($"You arent wearing an item '{name}'");
			}

			return item;
		}

		public static string JoinForUsage<T>() where T : struct, Enum
		{
			return string.Join('|', from e in Enum.GetValues<T>() select e.ToString().ToLower());
		}

		public static string BuildEnumString(this Type enumType)
		{
			var isNullable = false;
			if (enumType.IsNullableEnum())
			{
				enumType = enumType.GetNullableType();
				isNullable = true;
			}

			var values = Enum.GetValues(enumType);
			var list = new List<string>();

			foreach (var v in values)
			{
				list.Add(v.ToString().ToLower());
			}

			if (isNullable)
			{
				list.Add("null");
			}

			return string.Join('|', list);
		}

		public static string BuildEnumString<T>() where T : struct, Enum => typeof(T).BuildEnumString();

		private static T EnsureById<T, TKey>(this ExecutionContext context, TKey id, Func<TKey, T> getter)
		{
			var item = getter(id);
			if (item == null)
			{
				var name = typeof(T).Name.ToLower();
				context.Send($"Unable to find {name} with id '{id}'");
			}

			return item;

		}

		public static Mobile EnsureMobileById(this ExecutionContext context, int id) => EnsureById(context, id, Mobile.GetMobileById);
		public static Character EnsureCharacterByName(this ExecutionContext context, string name) => EnsureById(context, name, Character.GetCharacterByName);

		public static Item EnsureItemType(this ExecutionContext context, string id, ItemType itemType)
		{
			var item = Item.EnsureItemById(id);
			if (item == null)
			{
				return null;
			}

			if (item.ItemType != itemType)
			{
				context.Send($"Item {item} isnt {itemType.ToString().ToLower()}");
				return null;
			}

			return item;
		}

		public static AsciiGrid BuildEquipmentDesc(this Creature creature)
		{
			AsciiGrid grid = null;

			var y = 0;
			foreach (var eq in creature.Equipment.Items)
			{
				if (eq.Item == null)
				{
					continue;
				}

				if (grid == null)
				{
					grid = new AsciiGrid();
				}

				grid.SetValue(0, y, $"<worn on {eq.Slot.ToString().ToLower()}>");
				grid.SetValue(1, y, eq.Item.ShortDescription);

				++y;
			}

			return grid;
		}

		public static string GetStringId(this object entity)
		{
			var asStringEntity = entity as IHasId<string>;
			if (asStringEntity != null)
			{
				return asStringEntity.Id;
			}

			var asIntEntity = entity as IHasId<int>;
			if (asIntEntity != null)
			{
				return asIntEntity.Id.ToString();
			}

			throw new Exception($"Can't determine id of {entity}");
		}

		public static void SetStringId(this ExecutionContext context, object entity, string value)
		{
			var asStringEntity = entity as IHasId<string>;
			if (asStringEntity != null)
			{
				asStringEntity.Id = value;
			}

			var asIntEntity = entity as IHasId<int>;
			if (asIntEntity != null)
			{
				int id;
				if (!context.EnsureInt(value, out id))
				{
					return;
				}

				asIntEntity.Id = id;
			}

			throw new Exception($"Can't determine id of {entity}");
		}

		public static void SaveObject(this ExecutionContext context, object item)
		{
			do
			{
				var asStoredInFile = item as IStoredInFile;
				if (asStoredInFile != null)
				{
					asStoredInFile.Save();
					break;
				}

				var asAreaEntity = item as AreaEntity;
				if (asAreaEntity != null)
				{
					asAreaEntity.Area.Save();
					break;
				}

				context.Send($"ERROR: Unable to save entity of type {item.GetType().Name}");
			}
			while (false);
		}

		public static void GetItemWearTerms(this ItemType itemType, out string term1, out string term2, out string term3)
		{
			if (itemType == ItemType.Weapon)
			{
				term1 = "wield";
				term2 = "wielded";
				term3 = "wields";

				return;
			}

			term1 = "wear";
			term2 = "worn";
			term3 = "wears";
		}

		public static bool WearItem(this ExecutionContext context, ItemInstance item)
		{
			var result = context.Creature.Wear(item);

			string term1, term2, term3;
			item.ItemType.GetItemWearTerms(out term1, out term2, out term3);
			if (result == true)
			{
				// Remove from inv
				context.Creature.Inventory.AddItem(item, -1);
				context.Send($"You {term1} {item.ShortDescription}.");
				context.SendRoomExceptMe($"{context.Creature.ShortDescription} {term3} {item.ShortDescription}.");

				return true;
			}
			else if (result == false)
			{
				context.Send($"You can't {term1} {item.ShortDescription}, since that slot is occupied.");
				return false;
			}

			context.Send($"{item.ShortDescription} can't be {term2}.");
			return false;
		}

		public static bool RemoveItem(this ExecutionContext context, SlotType slot)
		{
			var removedItem = context.Creature.Remove(slot);
			if (removedItem != null)
			{
				// Add to inv
				context.Creature.Inventory.AddItem(removedItem, 1);

				var term = slot == SlotType.Wield ? "wielding" : "wearing";
				context.Send($"You stop {term} {removedItem.ShortDescription}.");

				context.SendRoomExceptMe($"{context.Creature.ShortDescription} stops {term} {removedItem.ShortDescription}.");

				return true;
			}

			return false;
		}

		public static ItemInstance FindStabWeapon(this Creature creature, string data, out bool isWielded)
		{
			isWielded = false;
			if (string.IsNullOrEmpty(data))
			{
				return null;
			}

			var weapon = creature.Equipment[SlotType.Wield];
			if (weapon == null || !weapon.MatchesKeyword(data) || !weapon.Info.Flags.Contains(ItemFlags.Stab))
			{
				// Check if there's such item in the inventory
				var inv = (from i in creature.Inventory.Items
						   where i.Item.MatchesKeyword(data) && i.Item.ItemType == ItemType.Weapon && i.Item.Info.Flags.Contains(ItemFlags.Stab)
						   select i).FirstOrDefault();
				if (inv == null)
				{
					return null;
				}

				weapon = inv.Item;
			}
			else
			{
				isWielded = true;
			}

			return weapon;
		}

		public static ExecutionContext GetContext(this Creature creature) => (ExecutionContext)creature.Tag;
	}
}