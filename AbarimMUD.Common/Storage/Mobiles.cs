using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Mobiles : MultipleFilesStorageString<Mobile>
	{
		public Mobiles() : base(m => m.Id, "mobiles")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();
			result.Converters.Add(Common.RaceConverter);
			result.Converters.Add(Common.ClassConverter);
			result.Converters.Add(Common.ItemConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var mobile in this)
			{
				mobile.Race = Race.EnsureRaceById(mobile.Race.Id);
				mobile.Class = GameClass.EnsureClassById(mobile.Class.Id);
			}
		}
	}
}
