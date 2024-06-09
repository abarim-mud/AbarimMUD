using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal class GameClasses : MultipleFilesStorageString<GameClass>
	{
		private class SkillConverter : JsonConverter<Skill>
		{
			public override Skill Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new Skill
			{
				Name = reader.GetString()
			};

			public override void Write(Utf8JsonWriter writer, Skill value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);
		}

		public GameClasses() : base(c => c.Name, "classes")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(new SkillConverter());

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach(var cls in this)
			{
				var newDict = new Dictionary<int, List<Skill>>();
				foreach (var pair in cls.SkillsByLevels)
				{
					var newSkills = new List<Skill>();
					foreach(var skill in pair.Value)
					{
						newSkills.Add(Skill.EnsureSkillByName(skill.Name));
					}

					newDict[pair.Key] = newSkills;
				}

				cls.SkillsByLevels = newDict;
			}
		}
	}
}
