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
			var item = context.EnsureItemById(storage, itemId);
			if (item == null)
			{
				context.Send($"Unable to find item of type {objectType} by id '{itemId}'");
				return;
			}

			if (!property.SetStringValue(context, item, parts[3]))
			{
				return;
			}

			// Save
			context.SaveObject(item);
			context.Send($"Changed {item.GetStringId()}'s {property.Name} to '{parts[3]}'");
		}
	}
}
