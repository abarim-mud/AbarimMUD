using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class EntityConverter<ObjectType> : JsonConverter<ObjectType> where ObjectType : class, new()
		{
			private readonly Func<ObjectType, string> _getter;
			private readonly Action<ObjectType, string> _setter;

			public EntityConverter(Func<ObjectType, string> getter, Action<ObjectType, string> setter)
			{
				_getter = getter;
				_setter = setter;
			}

			public override ObjectType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var value = reader.GetString();
				if (string.IsNullOrEmpty(value))
				{
					return null;
				}

				var result = new ObjectType();
				_setter(result, value);

				return result;
			}

			public override void Write(Utf8JsonWriter writer, ObjectType value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(_getter(value));
			}
		}

		public class GameClassConverter: EntityConverter<GameClass>
		{
			public GameClassConverter(): base(e => e.Id, (e, v) => e.Id = v)
			{
			}
		}

		public static readonly EntityConverter<GameClass> ClassConverter = new GameClassConverter();
		public static readonly EntityConverter<Skill> SkillConverter = new EntityConverter<Skill>(e => e.Id, (e, v) => e.Id = v);
		public static readonly EntityConverter<Item> ItemConverter = new EntityConverter<Item>(e => e.Id, (e, v) => e.Id = v);
	}
}
