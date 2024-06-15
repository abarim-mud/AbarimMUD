using AbarimMUD.Commands.Builder.OLCUtils;
using System;

namespace AbarimMUD.Commands.Builder
{
	public class CreateCopy : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: createcopy {OLCManager.KeysString} _templateId_ [_id_]");
				return;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return;
			}

			if (parts.Length < 2)
			{
				if (!storage.RequiresId)
				{
					context.Send($"Usage: createcopy {objectType} _templateId_");
				} else
				{
					context.Send($"Usage: createcopy {objectType} _templateId_ _id_");
				}

				return;
			}

			var objId = parts[1].ToLower();
			var obj = context.EnsureItemById(storage, objId);
			if (obj == null)
			{
				return;
			}

			var newId = string.Empty;
			if (storage.RequiresId)
			{
				if (parts.Length < 3)
				{
					context.Send($"Usage: createcopy {objectType} _templateId_ _id_");
					return;
				}

				newId = parts[2];
			}

			var asCloneable = obj as ICloneable;
			if (asCloneable == null)
			{
				context.Send($"Object of type {objectType} can't be copied.");
				return;
			}

			var newObject = asCloneable.Clone();
			if (!string.IsNullOrEmpty(newId))
			{
				var editor = ClassEditor.GetEditor(storage.ObjectType);
				var property = editor.FindByName("Id");
				if (property == null)
				{
					context.Send($"Object of type {objectType} lacks 'Id' property.");
					return;
				}

				property.SetStringValue(context, newObject, newId);
			}

			context.SaveObject(newObject);
			context.Send($"Created new object {newObject.GetStringId()} of type {objectType}");
		}
	}
}
