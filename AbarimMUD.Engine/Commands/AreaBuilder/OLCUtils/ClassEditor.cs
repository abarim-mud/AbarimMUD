using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AbarimMUD.Commands.AreaBuilder.OLCUtils
{
	public class ClassEditor
	{
		private static readonly Dictionary<Type, ClassEditor> _classes = new Dictionary<Type, ClassEditor>();

		private readonly Dictionary<string, Record> _records = new Dictionary<string, Record>();

		public Type EditedType { get; private set; }

		public IReadOnlyDictionary<string, Record> Records => _records;

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
		}

		private static bool CanBeAdded(Type type)
		{
			if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
			{
				return false;
			}

			return true;
		}

		public static ClassEditor GetEditor<T>()
		{
			ClassEditor result;

			var type = typeof(T);
			if (!_classes.TryGetValue(type, out result))
			{
				result = new ClassEditor(type);
				_classes[type] = result;
			}

			return result;
		}

		public Record FindByName(string name)
		{
			Record record;
			if (_records.TryGetValue(name.ToLower(), out record))
			{
				return record;
			}

			return null;
		}
	}
}
