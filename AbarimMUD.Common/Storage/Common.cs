using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class EntityConverter<ObjectType> : JsonConverter<ObjectType> where ObjectType : class, IHasId<string>, new()
		{
			private readonly Func<ObjectType, string> _objToId;

			public EntityConverter(Func<ObjectType, string> objToId)
			{
				_objToId = objToId ?? throw new ArgumentNullException(nameof(objToId));
			}

			public override ObjectType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var value = reader.GetString();
				if (string.IsNullOrEmpty(value))
				{
					return null;
				}

				var result = new ObjectType
				{
					Id = value
				};

				return result;
			}

			public override void Write(Utf8JsonWriter writer, ObjectType value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(_objToId(value));
			}
		}

		public class MobileClassConverterType : EntityConverter<MobileClass>
		{
			public MobileClassConverterType() : base(c => c.Id)
			{
			}
		}

		public class SkillValueConverterType : JsonConverter<SkillValue>
		{
			public override SkillValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				// It should contain only skill level
				// The id is key in the dictionary
				var value = reader.GetString();

				var level = Enum.Parse<SkillLevel>(value);

				return new SkillValue
				{
					Level = level
				};
			}

			public override void Write(Utf8JsonWriter writer, SkillValue value, JsonSerializerOptions options)
			{
				// Write only level
				// The skill id is key of the dict
				writer.WriteStringValue(value.Level.ToString());
			}
		}

		public static readonly EntityConverter<MobileClass> MobileClassConverter = new MobileClassConverterType();
		public static readonly EntityConverter<PlayerClass> PlayerClassConverter = new EntityConverter<PlayerClass>(s => s.Id);
		public static readonly EntityConverter<Skill> SkillConverter = new EntityConverter<Skill>(s => s.Id);
		public static readonly EntityConverter<Item> ItemConverter = new EntityConverter<Item>(i => i.Id);
		public static readonly EntityConverter<Ability> AbilityConverter = new EntityConverter<Ability>(s => s.Id);
		public static readonly SkillValueConverterType SkillValueConverter = new SkillValueConverterType();
	}
}
