using AbarimMUD.Data;
using System.Text.Json;
using Ur;

namespace AbarimMUD.Storage
{
	internal class Items : MultipleFilesStorage<Item>
	{
		public Items() : base("items")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.AffectsConverter);
			result.Converters.Add(Common.AbilityConverter);

			return result;
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach(var item in this)
			{
				if (item.Ability != null)
				{
					item.Ability.SetReferences();
				}
			}
		}
	}
}
