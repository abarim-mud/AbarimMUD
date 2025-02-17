using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Forges : MultipleFilesStorage<Forge>
	{
		public Forges() : base(m => m.Id, "forges")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemConverter);
			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var forge in this)
			{
				forge.Components.SetReferences();
				forge.Result = Item.EnsureItemById(forge.Result.Id);
			}
		}
	}
}
