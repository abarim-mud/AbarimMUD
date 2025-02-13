using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class LootRecord
	{
		public Inventory Items { get; set; }
		public int ProbabilityPercentage { get; set; }

		public LootRecord()
		{
		}

		public LootRecord(Inventory items, int probabilityPercentage)
		{
			this.Items = items;
			ProbabilityPercentage = probabilityPercentage;
		}

		public LootRecord Clone() => new LootRecord
		{
			Items = Items.Clone(),
			ProbabilityPercentage = ProbabilityPercentage
		};
	}

	public class GenericLootRecord
	{
		public int ProbabilityPercentage { get; set; }
		public LootRecord[] Choice { get; set; }
	}

	public class GenericLoot
	{
		internal static readonly CustomStorage<GenericLoot> Storage = new GenericLoots();

		public SortedDictionary<int, GenericLootRecord[]> Data { get; set; } = new SortedDictionary<int, GenericLootRecord[]>();

		public static IReadOnlyDictionary<int, GenericLootRecord[]> Items => Storage.Item.Data;

		public static void Save() => Storage.Save();
	}
}
