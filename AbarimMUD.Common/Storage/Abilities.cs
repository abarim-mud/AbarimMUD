using AbarimMUD.Data;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Abilities : MultipleFilesStorage<Ability>
	{
		public Abilities() : base(c => c.Id, "abilities")
		{
		}

		protected override string BuildPath(Ability entity)
		{
			if (entity.Type == AbilityType.Physical)
			{
				var key = GetKey(entity);
				var folder = Path.Combine(Folder, "physical");
				return Path.ChangeExtension(Path.Combine(folder, key), "json");
			}

			if (entity.Type == AbilityType.Spell)
			{
				var key = GetKey(entity);
				var folder = Path.Combine(Folder, "spells");
				return Path.ChangeExtension(Path.Combine(folder, key), "json");
			}

			return base.BuildPath(entity);
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
