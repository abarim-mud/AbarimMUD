using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class ForgeShops: MultipleFilesStorage<ForgeShop>
	{
		public ForgeShops(): base(f => f.Id, "forgeShops")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach(var forgeShop in this)
			{
				for(var i = 0; i < forgeShop.Forges.Count; ++i)
				{
					var forge = forgeShop.Forges[i];
					forge.Components.SetReferences();
					forge.Result = Item.EnsureItemById(forge.Result.Id);
				}
			}
		}
	}
}
