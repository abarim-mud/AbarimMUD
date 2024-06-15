using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AbarimMUD.Data;
using System.Text.Json.Serialization;
using AbarimMUD.Attributes;

namespace AbarimMUD.Commands.Builder.OLCUtils
{
	public class ClassEditor
	{
		private static readonly Dictionary<Type, ClassEditor> _classes = new Dictionary<Type, ClassEditor>();

		private readonly Dictionary<string, IRecord> _records = new Dictionary<string, IRecord>();
		private string _propsString = null;

		public Type EditedType { get; private set; }

		public string PropertiesString
		{
			get
			{
				if (_propsString == null)
				{
					_propsString = string.Join('|', from pair in _records select pair.Key);
				}

				return _propsString;
			}
		}

		private ClassEditor(Type type)
		{
			EditedType = type ?? throw new ArgumentNullException(nameof(type));

			// Add properties
			var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			foreach (var prop in props)
			{
				if (!CanBeAdded(prop) || !CanBeAdded(prop.PropertyType))
				{
					continue;
				}

				if (!prop.CanRead || !prop.CanWrite)
				{
					continue;
				}

				var name = prop.Name.ToLower();
				var olcAlias = prop.FindAttribute<OLCAliasAttribute>();
				if (olcAlias != null)
				{
					name = olcAlias.Alias;
				}

				_records[name] = new PropertyRecord(prop);
			}

			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (var field in fields)
			{
				if (!CanBeAdded(field) || !CanBeAdded(field.FieldType))
				{
					continue;
				}

				var name = field.Name.ToLower();
				var olcAlias = field.FindAttribute<OLCAliasAttribute>();
				if (olcAlias != null)
				{
					name = olcAlias.Alias;
				}

				_records[name] = new FieldRecord(field);
			}
		}

		private static bool CanBeAdded(Type type)
		{
			if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
			{
				return false;
			}

			return true;
		}

		private static bool CanBeAdded(MemberInfo memberInfo)
		{
			return !memberInfo.HasAttribute<OLCIgnoreAttribute>() &&
				!memberInfo.HasAttribute<JsonIgnoreAttribute>();
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

		public void RegisterCustomEditor(string name, string paramsStr, SetStringValuesDelegate setter)
		{
			if (FindByName(name) != null)
			{
				throw new Exception($"Can't register custom editor for property {name}, since there's a record");
			}

			_records[name] = new DelegateRecord(name, paramsStr, setter);
			_propsString = null;
		}

		public IRecord FindByName(string name)
		{
			IRecord record;
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
				var irec = pair.Value;
				var rec = irec as ReflectionRecord;

				if (rec == null)
				{
					continue;
				}

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