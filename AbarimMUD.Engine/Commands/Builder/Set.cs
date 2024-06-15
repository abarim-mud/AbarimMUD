using AbarimMUD.Commands.Builder.OLCUtils;

namespace AbarimMUD.Commands.Builder
{
	public class Set : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
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

			if (parts.Length < 4)
			{
				var p = "_params_";

				if (property.Type.IsEnum || property.Type.IsNullableEnum())
				{
					p = property.Type.BuildEnumString();
				}

				context.Send($"Usage: set {objectType} {propertyName} {p} _id_");
				return;
			}

			var newValue = parts[2];

			var objectId = parts[3].ToLower();
			var obj = context.EnsureItemById(storage, objectId);
			if (obj == null)
			{
				context.Send($"Unable to find item of type {objectType} by id '{objectId}'");
				return;
			}

			if (!property.SetStringValue(context, obj, newValue))
			{
				return;
			}

			// Save
			context.SaveObject(obj);
			context.Send($"Changed {obj.GetStringId()}'s {property.Name} to '{parts[3]}'");
		}
	}
}