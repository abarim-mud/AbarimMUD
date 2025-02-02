using AbarimMUD.Data;
using System;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Skills: MultipleFilesStorage<Skill>
	{
		public Skills() : base(c => c.Id, "skills")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.AbilityConverter);
			result.Converters.Add(Common.PlayerClassConverter);

			return result;
		}

		protected internal override void SetReferences()
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
					if (def.Abilities != null)
					{
						for(var i = 0; i < def.Abilities.Length; ++i)
						{
							def.Abilities[i] = Ability.EnsureAbilityById(def.Abilities[i].Id);
						}
					}
				}
			}
		}
	}
}
