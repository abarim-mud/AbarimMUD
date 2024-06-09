using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class ItemWithNameConverter<T> : JsonConverter<T> where T : new()
		{
			private readonly Func<T, string> _getter;
			private readonly Action<T, string> _setter;

			public ItemWithNameConverter(Func<T, string> getter, Action<T, string> setter)
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

		public static readonly ItemWithNameConverter<Race> RaceConverter = new ItemWithNameConverter<Race>(item => item.Name, (item, n) => item.Name = n);
		public static readonly ItemWithNameConverter<GameClass> ClassConverter = new ItemWithNameConverter<GameClass>(item => item.Name, (item, n) => item.Name = n);
		public static readonly ItemWithNameConverter<Skill> SkillConverter = new ItemWithNameConverter<Skill>(item => item.Name, (item, n) => item.Name = n);
	}
}
