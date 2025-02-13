﻿using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
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

		public class AffectsConverterType: JsonConverter<Dictionary<ModifierType, Affect>>
		{
			private static JsonSerializerOptions _defaultOptions;

			public static JsonSerializerOptions DefaultOptions
			{
				get
				{
					if (_defaultOptions == null)
					{
						_defaultOptions = JsonUtils.CreateOptions();
					}

					return _defaultOptions;
				}
			}

			public override Dictionary<ModifierType, Affect> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				var result = new Dictionary<ModifierType, Affect>();
				var doc = JsonDocument.ParseValue(ref reader);

				foreach(var pair in doc.RootElement.EnumerateObject())
				{
					var modifier = Enum.Parse<ModifierType>(pair.Name);

					if (pair.Value.ValueKind == JsonValueKind.Number)
					{
						// No duration
						result[modifier] = new Affect(modifier, pair.Value.GetInt32());
					} else
					{
						// Duration
						result[modifier] = JsonSerializer.Deserialize<Affect>(pair.Value, DefaultOptions);
						result[modifier].Type = modifier;
					}
				}

				return result;
			}

			public override void Write(Utf8JsonWriter writer, Dictionary<ModifierType, Affect> value, JsonSerializerOptions options)
			{
				var newDict = new Dictionary<ModifierType, object>();
				
				foreach(var pair in value)
				{
					// If there's no duration, then write just value
					if (pair.Value.DurationInSeconds == null)
					{
						newDict[pair.Key] = pair.Value.Value;
					}  else
					{
						newDict[pair.Key] = pair.Value;
					}
				}

				JsonSerializer.Serialize(writer, newDict, DefaultOptions);
			}
		}

		public class ItemInstanceConverterType : JsonConverter<ItemInstance>
		{
			private static JsonSerializerOptions _defaultOptions;

			public static JsonSerializerOptions DefaultOptions
			{
				get
				{
					if (_defaultOptions == null)
					{
						_defaultOptions = JsonUtils.CreateOptions();
						_defaultOptions.Converters.Add(ItemConverter);
					}

					return _defaultOptions;
				}
			}


			public override ItemInstance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if (reader.TokenType == JsonTokenType.String)
				{
					// Just an id
					var itemId = reader.GetString();

					return new ItemInstance
					{
						Info = new Item { Id = itemId }
					};
				}

				// Standard parse
				var doc = JsonDocument.ParseValue(ref reader);
				return JsonSerializer.Deserialize<ItemInstance>(doc, DefaultOptions);
			}

			public override void Write(Utf8JsonWriter writer, ItemInstance value, JsonSerializerOptions options)
			{
				// Write just an id
				writer.WriteStringValue(value.Id);
			}
		}

		public static readonly EntityConverter<PlayerClass> PlayerClassConverter = new EntityConverter<PlayerClass>(s => s.Id);
		public static readonly EntityConverter<Skill> SkillConverter = new EntityConverter<Skill>(s => s.Id);
		public static readonly EntityConverter<Item> ItemConverter = new EntityConverter<Item>(i => i.Id);
		public static readonly ItemInstanceConverterType ItemInstanceConverter = new ItemInstanceConverterType();
		public static readonly EntityConverter<Ability> AbilityConverter = new EntityConverter<Ability>(s => s.Id);
		public static readonly SkillValueConverterType SkillValueConverter = new SkillValueConverterType();
		public static readonly AffectsConverterType AffectsConverter = new AffectsConverterType();
	}
}
