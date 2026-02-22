using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Mobiles: MultipleFilesStorage<Mobile>
	{
		public Mobiles(): base(t => t.Id, "mobiles")
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

			foreach(var mobileTemplate in this)
			{
				if (mobileTemplate.Loot != null)
				{
					foreach (var loot in mobileTemplate.Loot)
					{
						loot.Items.SetReferences();
					}
				}
			}
		}
	}
}
