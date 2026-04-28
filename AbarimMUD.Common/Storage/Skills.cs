using AbarimMUD.Data;
using System;
using System.Text.Json;
using Ur;

namespace AbarimMUD.Storage
{
	internal class Skills: MultipleFilesStorage<Skill>
	{
		public Skills() : base("skills")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.AbilitiesConverter);

			return result;
		}

		protected override void SetReferences()
		{
			base.SetReferences();

			foreach (var skill in this)
			{
				if (skill.Class == null)
				{
					throw new Exception($"Skill {skill.Id} doesn't have Class.");
				}

				skill.Class = PlayerClass.EnsureClassById(skill.Class.Id);

				foreach(var def in skill.Levels)
				{
					foreach(var pair in def.Abilities)
					{
						pair.Value.Ability = Ability.EnsureAbilityById(pair.Key);
					}
				}
			}
		}
	}
}
