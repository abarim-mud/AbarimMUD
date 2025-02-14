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

			result.Converters.Add(Common.ForgeConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach(var forgeShop in this)
			{
				for(var i = 0; i < forgeShop.Forges.Count; ++i)
				{
					forgeShop.Forges[i] = Forge.EnsureForgeById(forgeShop.Forges[i].Id);
				}
			}
		}
	}
}
