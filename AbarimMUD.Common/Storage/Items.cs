using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Items : MultipleFilesStorage<Item>
	{
		public Items() : base(m => m.Id, "items")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.AffectsConverter);

			return result;
		}
	}
}
