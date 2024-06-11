using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public abstract class BaseEntityConverter<KeyType, ObjectType> : JsonConverter<ObjectType> where ObjectType : new()
		{
			private readonly Func<ObjectType, KeyType> _getter;
			private readonly Action<ObjectType, KeyType> _setter;

			protected BaseEntityConverter(Func<ObjectType, KeyType> getter, Action<ObjectType, KeyType> setter)
			{
				_getter = getter;
				_setter = setter;
			}

			public override ObjectType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var result = new ObjectType();
				_setter(result, ReadValue(ref reader));

				return result;
			}

			public override void Write(Utf8JsonWriter writer, ObjectType value, JsonSerializerOptions options)
			{
				WriteValue(writer, _getter(value));
			}

			protected abstract KeyType ReadValue(ref Utf8JsonReader reader);
			protected abstract void WriteValue(Utf8JsonWriter writer, KeyType value);
		}

		public class StringEntityConverter<ObjectType> : BaseEntityConverter<string, ObjectType> where ObjectType : new()
		{
			public StringEntityConverter(Func<ObjectType, string> getter, Action<ObjectType, string> setter) : base(getter, setter)
			{
			}

			protected override string ReadValue(ref Utf8JsonReader reader) => reader.GetString();

			protected override void WriteValue(Utf8JsonWriter writer, string value) => writer.WriteStringValue(value);
		}

		public class IntEntityConverter<ObjectType> : BaseEntityConverter<int, ObjectType> where ObjectType : new()
		{
			public IntEntityConverter(Func<ObjectType, int> getter, Action<ObjectType, int> setter) : base(getter, setter)
			{
			}

			protected override int ReadValue(ref Utf8JsonReader reader) => reader.GetInt32();

			protected override void WriteValue(Utf8JsonWriter writer, int value) => writer.WriteNumberValue(value);
		}

		public static readonly StringEntityConverter<Race> RaceConverter = new StringEntityConverter<Race>(e => e.Id, (e, v) => e.Name = v);
		public static readonly StringEntityConverter<GameClass> ClassConverter = new StringEntityConverter<GameClass>(e => e.Id, (e, v) => e.Name = v);
		public static readonly StringEntityConverter<Skill> SkillConverter = new StringEntityConverter<Skill>(e => e.Id, (e, v) => e.Name = v);
		public static readonly StringEntityConverter<Item> ItemConverter = new StringEntityConverter<Item>(e => e.Id, (e, v) => e.Id = v);
	}
}
