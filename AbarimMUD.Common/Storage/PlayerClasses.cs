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

			result.Converters.Add(Common.ItemInstanceConverter);
			result.Converters.Add(Common.SkillConverter);
			result.Converters.Add(Common.InventoryConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var cls in this)
			{
				if (cls.StartingEquipment != null)
				{
					for (var i = 0; i < cls.StartingEquipment.Count; ++i)
					{
						cls.StartingEquipment[i].Info = Item.EnsureItemById(cls.StartingEquipment[i].Id);
					}
				}

				if (cls.StartingSkills != null)
				{
					for (var i = 0; i < cls.StartingSkills.Count; ++i)
					{
						cls.StartingSkills[i] = Skill.EnsureSkillById(cls.StartingSkills[i].Id);
					}
				}

				if (cls.StartingInventory != null)
				{
					for (var i = 0; i < cls.StartingInventory.Items.Count; ++i)
					{
						cls.StartingInventory.Items[i].Info = Item.EnsureItemById(cls.StartingInventory.Items[i].Id);
					}
				}
			}
		}
	}
}
