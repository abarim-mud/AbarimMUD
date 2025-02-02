using AbarimMUD.Data;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Abilities : MultipleFilesStorage<Ability>
	{
		public Abilities() : base(c => c.Id, "abilities")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.PlayerClassConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach(var ability in this)
			{
				if (ability.PrimeClass != null)
				{
					ability.PrimeClass = PlayerClass.EnsureClassById(ability.PrimeClass.Id);
				}
			}
		}
	}
}
