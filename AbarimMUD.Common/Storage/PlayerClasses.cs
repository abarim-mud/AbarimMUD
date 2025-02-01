using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class PlayerClasses : MultipleFilesStorage<PlayerClass>
	{
		public PlayerClasses() : base(c => c.Id, "playerClasses")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemConverter);

			return result;
		}
	}
}
