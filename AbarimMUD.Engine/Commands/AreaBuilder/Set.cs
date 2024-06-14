using AbarimMUD.Commands.AreaBuilder.OLCUtils;
using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class Set : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: set {OLCManager.KeysString} _propertyName_ _id_ _params_");
				return;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return;
			}

			var editor = ClassEditor.GetEditor(storage.ObjectType);
			if (parts.Length < 2)
			{
				context.Send($"Usage: set {objectType} {editor.PropertiesString} _id_ _params_");
				return;
			}

			var propertyName = parts[1].ToLower();
			var property = editor.FindByName(propertyName);
			if (property == null)
			{
				context.Send($"Unable to find property {propertyName} in object of type {objectType}");
				return;
			}

			if (parts.Length < 4)
			{
				context.Send($"Usage: set {objectType} {propertyName} _id_ _params_");
				return;
			}

			var itemId = parts[2].ToLower();
			var item = storage.FindById(context, itemId);
			if (item == null)
			{
				context.Send($"Unable to find item of type {objectType} by id '{itemId}'");
				return;
			}

			var s = parts[3];
			if (s.EqualsToIgnoreCase("null"))
			{
				if (property.Type.IsClass || property.Type.IsNullable())
				{
					property.SetValue(item, null);
				}
				else
				{
					context.Send($"Property {property.Name} of type '{property.Type.Name}' can't be set to null.");
					return;
				}
			}
			else if (property.Type == typeof(string))
			{
				property.SetValue(item, s);
			}
			else if (property.Type == typeof(bool) || property.Type == typeof(bool?))
			{
				bool b;
				if (!context.EnsureBool(s, out b))
				{
					return;
				}

				property.SetValue(item, b);
			}
			else if (property.Type == typeof(RaceClassValueRange) || property.Type == typeof(RaceClassValueRange?))
			{
				var parts2 = s.SplitByWhitespace();
				if (parts2.Length < 2)
				{
					context.Send($"Usage: set {objectType} {propertyName} _id_ _level1Value_ _level100Value_");
					return;
				}

				int level1Value;
				if (!context.EnsureInt(parts2[0], out level1Value))
				{
					return;
				}

				int level100Value;
				if (!context.EnsureInt(parts2[1], out level100Value))
				{
					return;
				}

				property.SetValue(item, new RaceClassValueRange(level1Value, level100Value));
			}
			else if (property.Type == typeof(GameClass))
			{
				var cls = context.EnsureClassById(s);
				if (cls == null)
				{
					return;
				}

				if (ReferenceEquals(cls, item))
				{
					context.Send("Object can't reference itself.");
					return;
				}

				property.SetValue(item, cls);
			}
			else
			{
				context.Send($"Setting propertes of type '{property.Type.Name}' isn't implemented.");
				return;
			}

			// Save
			do
			{
				var asStoredInFile = item as IStoredInFile;
				if (asStoredInFile != null)
				{
					asStoredInFile.Save();
					break;
				}

				var asAreaEntity = item as AreaEntity;
				if (asAreaEntity != null)
				{
					asAreaEntity.Area.Save();
					break;
				}

				throw new Exception($"Unable to save entity of type {objectType}");
			}
			while (false);

			context.Send($"Changed {item.GetStringId()}'s {property.Name} to '{s}'");
		}
	}
}
