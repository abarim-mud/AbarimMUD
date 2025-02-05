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
			result.Converters.Add(Common.SkillConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var cls in this)
			{
				if (cls.StartingEquipment != null)
				{
					for (var i = 0; i < cls.StartingEquipment.Length; ++i)
					{
						cls.StartingEquipment[i] = Item.EnsureItemById(cls.StartingEquipment[i].Id);
					}
				}

				if (cls.StartingSkills != null)
				{
					for (var i = 0; i < cls.StartingSkills.Length; ++i)
					{
						cls.StartingSkills[i] = Skill.EnsureSkillById(cls.StartingSkills[i].Id);
					}
				}

				if (cls.StartingInventory != null)
				{
					for (var i = 0; i < cls.StartingInventory.Items.Length; ++i)
					{
						cls.StartingInventory.Items[i].Info = Item.EnsureItemById(cls.StartingInventory.Items[i].Id);
					}
				}
			}
		}
	}
}
