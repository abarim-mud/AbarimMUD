using AbarimMUD.Data;
using System;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class GenericLoots : CustomStorage<GenericLoot>
	{
		public GenericLoots() : base("genericLoot.json")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var pair in Item.Data)
			{
				foreach (var genericLootRecord in pair.Value)
				{
					var totalProb = 0;
					foreach (var lootRecord in genericLootRecord.Choice)
					{
						lootRecord.Items.SetReferences();
						totalProb += lootRecord.ProbabilityPercentage;
					}

					if (totalProb > 100)
					{
						throw new Exception($"Total probability {totalProb} exceeds 100%");
					}
				}
			}
		}
	}
}
