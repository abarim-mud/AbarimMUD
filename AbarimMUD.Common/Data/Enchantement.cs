using AbarimMUD.Storage;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class Enchantement : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<Enchantement> Storage = new MultipleFilesStorage<Enchantement>(e => e.Id, "enchantements");

		public string Id { get; set; }

		public string Name { get; set; }

		public HashSet<string> Keywords { get; set; } = new HashSet<string>();
		public int EnchantementStones { get; set; } = 5;
		public int Price { get; set; } = 1000;

		public Dictionary<ModifierType, int> Affects { get; set; } = new Dictionary<ModifierType, int>();

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public Enchantement CloneItem()
		{
			var result = new Enchantement
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

		public static Enchantement GetEnchantementById(string id) => Storage.GetByKey(id);
		public static Enchantement EnsureEnchantementById(string id) => Storage.EnsureByKey(id);
	}
}
