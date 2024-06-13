using AbarimMUD.Data;
using System.Collections.Generic;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class GameClasses : MultipleFilesStorageString<GameClass>
	{
		public GameClasses() : base(c => c.Id, "classes")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.SkillConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var cls in this)
			{
				var newDict = new Dictionary<int, List<Skill>>();
				foreach (var pair in cls.SkillsByLevels)
				{
					var newSkills = new List<Skill>();
					foreach (var skill in pair.Value)
					{
						newSkills.Add(Skill.EnsureSkillById(skill.Id));
					}

					newDict[pair.Key] = newSkills;
				}

				if (cls.Inherits != null)
				{
					cls.Inherits = GameClass.EnsureClassById(cls.Inherits.Id);
				}

				cls.SkillsByLevels = newDict;
			}
		}
	}
}
