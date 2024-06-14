using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder.OLCUtils
{
	public class ClassEditor
	{
		private static readonly Dictionary<Type, ClassEditor> _classes = new Dictionary<Type, ClassEditor>();

		private readonly Dictionary<string, Record> _records = new Dictionary<string, Record>();
		private readonly string _propsString;

		public Type EditedType { get; private set; }

		public IReadOnlyDictionary<string, Record> Records => _records;

		public string PropertiesString => _propsString;

		private ClassEditor(Type type)
		{
			EditedType = type ?? throw new ArgumentNullException(nameof(type));

			// Add properties
			var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.GetProperty | BindingFlags.SetProperty);

			foreach (var prop in props)
			{
				if (!CanBeAdded(prop.PropertyType))
				{
					continue;
				}

				_records[prop.Name.ToLower()] = new PropertyRecord(prop);
			}

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public |
				BindingFlags.SetField | BindingFlags.GetField);
			foreach (var field in fields)
			{
				if (!CanBeAdded(field.FieldType))
				{
					continue;
				}

				_records[field.Name.ToLower()] = new FieldRecord(field);
			}

			_propsString = string.Join('|', from pair in _records select pair.Key);
		}

		private static bool CanBeAdded(Type type)
		{
			if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
			{
				return false;
			}

			return true;
		}

		public static ClassEditor GetEditor(Type type)
		{
			ClassEditor result;

			if (!_classes.TryGetValue(type, out result))
			{
				result = new ClassEditor(type);
				_classes[type] = result;
			}

			return result;
		}

		public static ClassEditor GetEditor<T>() => GetEditor(typeof(T));

		public Record FindByName(string name)
		{
			Record record;
			if (_records.TryGetValue(name.ToLower(), out record))
			{
				return record;
			}

			return null;
		}

		public IReadOnlyDictionary<string, string> BuildInfoDict(object obj)
		{
			var values = new Dictionary<string, string>();
			foreach (var pair in _records)
			{
				var rec = pair.Value;
				var value = rec.GetValue(obj);

				var stringValue = string.Empty;

				do
				{
					if (value == null)
					{
						stringValue = string.Empty;
						break;
					}

					if (rec.Type == typeof(string))
					{
						stringValue = (string)value;
						break;
					}

					if (rec.Type.IsPrimitive || rec.Type.IsEnum)
					{
						stringValue = value.ToString();
						break;
					}

					var enumerable = value as IEnumerable;
					if (enumerable != null)
					{
						var query = from object v in enumerable select v.ToString();

						if (rec.Name.EqualsToIgnoreCase("keywords"))
						{
							stringValue = string.Join(" ", query);
						}
						else
						{
							stringValue = string.Join(", ", query);
						}

						break;
					}

					var entity = value as IHasId<string>;
					if (entity != null)
					{
						stringValue = entity.Id;
						break;
					}

					// Complicated type
					// Get class editor for it
					var subClassEditor = GetEditor(rec.Type);
					var subValues = subClassEditor.BuildInfoDict(value);
					stringValue = string.Join(", ", from pair2 in subValues select $"{pair2.Key} = {pair2.Value}");
				}
				while (false);

				values[rec.Name] = stringValue;
			}

			return values;
		}
	}
}