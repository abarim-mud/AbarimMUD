using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class Enchantment : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Enchantment> Storage = new MultipleFilesStorage<Enchantment>(e => e.Id, "enchantments");

		public string Id { get; set; }

		public string Name { get; set; }

		public HashSet<string> Keywords { get; set; } = new HashSet<string>();
		public int EnchantmentStones { get; set; } = 5;
		public int Price { get; set; } = 1000;

		public Dictionary<ModifierType, int> Affects { get; set; } = new Dictionary<ModifierType, int>();
		public HashSet<ItemType> ItemTypes { get; set; } = new HashSet<ItemType>();
		public HashSet<ItemMaterial> Materials { get; set; } = new HashSet<ItemMaterial>();


		[JsonIgnore]
		public bool HasItemTypesFilters => ItemTypes.Count > 0;

		[JsonIgnore]
		public bool HasMaterialsFilters => Materials.Count > 0;

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public Enchantment CloneItem()
		{
			var result = new Enchantment
			{
				Id = Id,
				Name = Name,
			};

			foreach(var word in Keywords)
			{
				result.Keywords.Add(word);
			}

			foreach (var pair in Affects)
			{
				result.Affects[pair.Key] = pair.Value;
			}

			return result;
		}

		public object Clone() => CloneItem();

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Enchantment GetEnchantmentById(string id) => Storage.GetByKey(id);
		public static Enchantment EnsureEnchantmentById(string id) => Storage.EnsureByKey(id);
	}
}
