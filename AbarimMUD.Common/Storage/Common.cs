using AbarimMUD.Data;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AbarimMUD.Storage
{
	internal static class Common
	{
		public class EntityConverter<ObjectType> : JsonConverter<ObjectType> where ObjectType : class
		{
			private readonly Func<ObjectType, string> _objToId;
			private readonly Func<string, ObjectType> _idToObj;

			public EntityConverter(Func<ObjectType, string> objToId, Func<string, ObjectType> idToObj)
			{
				_objToId = objToId ?? throw new ArgumentNullException(nameof(objToId));
				_idToObj = idToObj ?? throw new ArgumentNullException(nameof(idToObj));
			}

			public override ObjectType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var value = reader.GetString();
				if (string.IsNullOrEmpty(value))
				{
					return null;
				}

				return _idToObj(value);
			}

			public override void Write(Utf8JsonWriter writer, ObjectType value, JsonSerializerOptions options)
			{
				writer.WriteStringValue(_objToId(value));
			}
		}

		public class GameClassConverter: EntityConverter<MobileClass>
		{
			public GameClassConverter(): base(c => c.Id, id => MobileClass.EnsureClassById(id))
			{

			}
		}

		public static readonly EntityConverter<MobileClass> ClassConverter = new GameClassConverter();
		public static readonly EntityConverter<Skill> SkillConverter = new EntityConverter<Skill>(s => s.Id, id => Skill.EnsureSkillById(id));
		public static readonly EntityConverter<Item> ItemConverter = new EntityConverter<Item>(i => i.Id, id => Item.EnsureItemById(id));
	}
}
