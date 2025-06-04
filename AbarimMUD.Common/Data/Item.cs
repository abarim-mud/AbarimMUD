using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ItemType
	{
		Equipment,
		Potion,
		Scroll,
		Material,
		Enchant1,
		Enchant2,
		Enchant3,
		Enchant4,
		Enchant5,
	}

	public enum EquipmentSlotType
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

	public enum ItemFlags
	{
		Stab,
		HideFromEquipmentList,
	}

	public enum ItemMaterial
	{
		Leather,
		Iron,
		Mithril
	}

	public class Item : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Item> Storage = new Items();

		private ItemType _itemType;
		private int? _enchantmentTier = null;

		public string Id { get; set; }
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public ItemType ItemType
		{
			get => _itemType;

			set
			{
				if (value == _itemType)
				{
					return;
				}

				_itemType = value;
			}
		}

		public EquipmentSlotType? EquipmentSlot { get; set; }

		public int Price { get; set; } = 100;
		public AttackType? AttackType { get; set; }

		public int? EnchantmentTier
		{
			get => _enchantmentTier;

			set
			{
				if (value.HasValue &&
					(value.Value < 1 || value.Value > 5))
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_enchantmentTier = value.Value;
			}
		}

		public ItemMaterial? Material { get; set; }

		public Dictionary<ModifierType, Affect> Affects { get; set; } = new Dictionary<ModifierType, Affect>();


		public HashSet<ItemFlags> Flags { get; set; } = new HashSet<ItemFlags>();
		public ValueRange? DamageRange { get; set; }

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public Item CloneItem()
		{
			var result = new Item
			{
				Id = Id,
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				AttackType = AttackType,
				ItemType = ItemType,
				DamageRange = DamageRange,
				EnchantmentTier = EnchantmentTier,
				Material = Material,
			};

			foreach(var pair in Affects)
			{
				result.Affects[pair.Key] = pair.Value.Clone();
			}

			foreach (var flag in Flags)
			{
				result.Flags.Add(flag);
			}

			foreach (var keyword in Keywords)
			{
				result.Keywords.Add(keyword);
			}

			return result;
		}

		public object Clone() => CloneItem();

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		private void InvalidateCreaturesWithThisItem()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				foreach (var slot in creature.Equipment.Slots)
				{
					if (slot.Item == null)
					{
						continue;
					}

					if (slot.Item.Info.Id == Id)
					{
						creature.InvalidateStats();
						break;
					}
				}
			}
		}

		public static Item GetItemById(string id) => Storage.GetByKey(id);
		public static Item EnsureItemById(string id) => Storage.EnsureByKey(id);
	}

	public static class ItemExtensions
	{
		public static ItemType ToEnchantmentItemType(this int enchantmentTier)
		{
			switch(enchantmentTier)
			{
				case 1:
					return ItemType.Enchant1;
				case 2:
					return ItemType.Enchant2;
				case 3:
					return ItemType.Enchant3;
				case 4:
					return ItemType.Enchant4;
				case 5:
					return ItemType.Enchant5;
			}

			throw new Exception($"Wrong enchantment tier {enchantmentTier}");
		}
	}
}