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

			itemEditor.RegisterCustomEditor("armor", $"{typeof(ArmorType).BuildEnumString()} _armor_", SetArmor);
			itemEditor.RegisterCustomEditor("weapon", $"{typeof(AttackType).BuildEnumString()} _penetration_ _minimumDamage_ _maximumDamage_", SetWeapon);
		}

		private static bool SetArmor(ExecutionContext context, object obj, IReadOnlyList<string> values)
		{
			ArmorType armorType;
			if (!context.EnsureEnum(values[0], out armorType))
			{
				return false;
			}

			int armor;
			if (!context.EnsureInt(values[1], out armor))
			{
				return false;
			}

			var item = (Item)obj;
			item.SetArmor(armorType, armor);

			return true;
		}

		private static bool SetWeapon(ExecutionContext context, object obj, IReadOnlyList<string> values)
		{
			AttackType attackType;
			if (!context.EnsureEnum(values[0], out attackType))
			{
				return false;
			}

			int penetration;
			if (!context.EnsureInt(values[1], out penetration))
			{
				return false;
			}

			int minimumDamage;
			if (!context.EnsureInt(values[2], out minimumDamage))
			{
				return false;
			}

			int maximumDamage;
			if (!context.EnsureInt(values[3], out maximumDamage))
			{
				return false;
			}

			var item = (Item)obj;
			item.SetWeapon(attackType, penetration, minimumDamage, maximumDamage);

			return true;
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 1)
			{
				context.Send($"Usage: set {OLCManager.KeysString} _propertyName_ _params_ _id_");
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
				context.Send($"Usage: set {objectType} {editor.PropertiesString} _params_ _id_");
				return;
			}

			var propertyName = parts[1].ToLower();
			var property = editor.FindByName(propertyName);
			if (property == null)
			{
				context.Send($"Unable to find property {propertyName} in object of type {objectType}");
				return;
			}

			var paramsCount = property.ParamsString.SplitByWhitespace().Length;
			if (parts.Length < 3 + paramsCount)
			{
				context.Send($"Usage: set {objectType} {propertyName} {property.ParamsString} _id_");
				return;
			}

			var objectId = parts[parts.Length - 1].ToLower();
			var obj = context.EnsureItemById(storage, objectId);
			if (obj == null)
			{
				context.Send($"Unable to find item of type {objectType} by id '{objectId}'");
				return;
			}

			var args = new ArraySegment<string>(parts, 2, parts.Length - 3);
			if (!property.SetStringValue(context, obj, args))
			{
				return;
			}

			// Save
			context.SaveObject(obj);
			context.Send($"Changed {obj.GetStringId()}'s {property.Name} to '{string.Join(' ', args)}'");
		}
	}
}