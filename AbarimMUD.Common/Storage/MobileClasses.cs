using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal class MobileClasses : MultipleFilesStorage<MobileClass>
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
				}
				else
				{
					JsonSerializer.Serialize(writer, value);
				}
			}
		}

		private static readonly AttackInfoConverter AttackConverter = new AttackInfoConverter();

		public MobileClasses() : base(c => c.Id, "mobileClasses")
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();

			result.Converters.Add(Common.ItemConverter);
			result.Converters.Add(AttackConverter);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var cls in this)
			{
				if (cls.Inherits != null)
				{
					cls.Inherits = MobileClass.EnsureClassById(cls.Inherits.Id);
				}

				if (cls.Loot != null)
				{
					foreach (var eqSet in cls.Loot)
					{
						if (eqSet.Items != null)
						{
							for (var i = 0; i < eqSet.Items.Length; ++i)
							{
								eqSet.Items[i] = Item.GetItemById(eqSet.Items[i].Id);
							}
						}
					}
				}
			}
		}
	}
}
