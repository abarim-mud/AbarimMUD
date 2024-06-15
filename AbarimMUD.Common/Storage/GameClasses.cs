using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal class GameClasses : MultipleFilesStorageString<GameClass>
	{
		private class AttackInfoConverter : JsonConverter<AttackInfo>
		{
			public override AttackInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.StartObject)
				{
					var doc = JsonDocument.ParseValue(ref reader);
					return JsonSerializer.Deserialize<AttackInfo>(doc);
				}

				var level = reader.GetInt32();
				return new AttackInfo
				{
					MinimumLevel = level
				};
			}

			public override void Write(Utf8JsonWriter writer, AttackInfo value, JsonSerializerOptions options)
			{
				if (value.AttackType == null &&
					value.PenetrationRange == null &&
					value.MinimumDamageRange == null &&
					value.MaximumDamageRange == null)
				{
					writer.WriteNumberValue(value.MinimumLevel);
				} else
				{
					JsonSerializer.Serialize(writer, value);
				}
			}
		}

		private static readonly AttackInfoConverter AttackConverter = new AttackInfoConverter();

		public GameClasses() : base(c => c.Id, "classes")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.SkillConverter);
			result.Converters.Add(AttackConverter);

			return result;
		}

		protected override string BuildPath(GameClass entity)
		{
			var result = base.BuildPath(entity);

			var folder = Path.GetDirectoryName(result);
			var fileName = Path.GetFileName(result);
			if (entity.IsPlayerClass)
			{
				result = Path.Combine(folder, "player");
			} else
			{
				result = Path.Combine(folder, "mobile");
			}

			result = Path.Combine(result, fileName);

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

				foreach(var pair in newDict)
				{
					cls.SkillsByLevels[pair.Key] = pair.Value.ToArray();
				}
			}
		}
	}
}
