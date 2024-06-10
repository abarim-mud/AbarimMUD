using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class EntityWithNameConverter<T> : JsonConverter<T> where T : new()
		{
			private readonly Func<T, string> _getter;
			private readonly Action<T, string> _setter;

			public EntityWithNameConverter(Func<T, string> getter, Action<T, string> setter)
			{
				_getter = getter;
				_setter = setter;
			}

			public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var result = new T();
				_setter(result, reader.GetString());

				return result;
			}

			public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				var name = _getter(value);
				writer.WriteStringValue(name);
			}
		}

		public static readonly EntityWithNameConverter<Race> RaceConverter = new EntityWithNameConverter<Race>(e => e.Name, (e, n) => e.Name = n);
		public static readonly EntityWithNameConverter<GameClass> ClassConverter = new EntityWithNameConverter<GameClass>(e => e.Name, (e, n) => e.Name = n);
		public static readonly EntityWithNameConverter<Skill> SkillConverter = new EntityWithNameConverter<Skill>(e => e.Name, (e, n) => e.Name = n);
		public static readonly EntityWithNameConverter<Item> ItemConverter = new EntityWithNameConverter<Item>(e => e.Name, (e, n) => e.Name = n);
	}
}
