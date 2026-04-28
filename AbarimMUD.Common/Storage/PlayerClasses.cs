using AbarimMUD.Data;
using System.Text.Json;
using Ur;

namespace AbarimMUD.Storage
{
	internal class PlayerClasses : MultipleFilesStorage<PlayerClass>
	{
		public PlayerClasses() : base("playerClasses")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.InventoryConverter);
			result.Converters.Add(Common.AbilitiesConverter);

			return result;
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach (var cls in this)
			{
				cls.StartingEquipment?.SetReferences();

				if (cls.StartingSkills != null)
				{
					for (var i = 0; i < cls.StartingSkills.Count; ++i)
					{
						cls.StartingSkills[i] = Skill.EnsureSkillById(cls.StartingSkills[i].Id);
					}
				}

				cls.StartingInventory?.SetReferences();
			}
		}
	}
}
