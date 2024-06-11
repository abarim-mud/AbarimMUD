using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Items : MultipleFilesStorageString<Item>
	{
		public Items() : base(m => m.Id, "items")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();
		}
	}
}
