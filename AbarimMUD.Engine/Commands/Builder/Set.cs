using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Data;
using System;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder
{
	public class Set : BuilderCommand
	{
		static Set()
		{
			var itemEditor = ClassEditor.GetEditor<Item>();

			itemEditor.RegisterCustomEditor("armor", $"_armor_", SetArmor);
			itemEditor.RegisterCustomEditor("weapon", $"_penetration_ _minimumDamage_ _maximumDamage_", SetWeapon);
		}

		private static bool SetArmor(ExecutionContext context, object obj, IReadOnlyList<string> values)
		{
			int armor;
			if (!context.EnsureInt(values[0], out armor))
			{
				return false;
			}

			var item = (Item)obj;
			item.SetArmor(armor);

			return true;
		}

		private static bool SetWeapon(ExecutionContext context, object obj, IReadOnlyList<string> values)
		{
			int penetration;
			if (!context.EnsureInt(values[0], out penetration))
			{
				return false;
			}

			int minimumDamage;
			if (!context.EnsureInt(values[1], out minimumDamage))
			{
				return false;
			}

			int maximumDamage;
			if (!context.EnsureInt(values[2], out maximumDamage))
			{
				return false;
			}

			var item = (Item)obj;
			item.SetWeapon(penetration, minimumDamage, maximumDamage);

			return true;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 1)
			{
				context.Send($"Usage: set {OLCManager.KeysString} _propertyName_ _params_ _id_");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			var editor = ClassEditor.GetEditor(storage.ObjectType);
			if (parts.Length < 2)
			{
				context.Send($"Usage: set {objectType} {editor.PropertiesString} _params_ _id_");
				return false;
			}

			var propertyName = parts[1].ToLower();
			var property = editor.FindByName(propertyName);
			if (property == null)
			{
				context.Send($"Unable to find property {propertyName} in object of type {objectType}");
				return false;
			}

			var paramsString = property.ParamsString;
			var paramsCount = paramsString.SplitByWhitespace().Length;
			if (parts.Length < 3 + paramsCount)
			{
				context.Send($"Usage: set {objectType} {propertyName} {paramsString} _id_");
				return false;
			}

			var objectId = parts[parts.Length - 1].ToLower();
			var obj = context.EnsureItemById(storage, objectId);
			if (obj == null)
			{
				return false;
			}

			if (propertyName == "id")
			{
				// When changing id make sure it wont override existing item
				var newId = parts[2];
				if (storage.FindById(context, newId) != null)
				{
					context.Send($"Id {newId} is used already.");
					return false;
				}

			}

			var args = new ArraySegment<string>(parts, 2, parts.Length - 3);
			if (!property.SetStringValue(context, obj, args))
			{
				return false;
			}

			// Save
			context.SaveObject(obj);
			context.Send($"Changed {obj.GetStringId()}'s {property.Name} to '{string.Join(' ', args)}'");

			return true;
		}
	}
}