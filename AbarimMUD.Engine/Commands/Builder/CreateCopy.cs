using AbarimMUD.Commands.Builder.OLCUtils;
using System;

namespace AbarimMUD.Commands.Builder
{
	public class CreateCopy : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: createcopy {OLCManager.KeysString} _templateId_ [_id_]");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			if (parts.Length < 2)
			{
				if (!storage.RequiresId)
				{
					context.Send($"Usage: createcopy {objectType} _templateId_");
				}
				else
				{
					context.Send($"Usage: createcopy {objectType} _templateId_ _id_");
				}

				return false;
			}

			var objId = parts[1].ToLower();
			var obj = context.EnsureItemById(storage, objId);
			if (obj == null)
			{
				return false;
			}

			var newId = string.Empty;
			if (storage.RequiresId)
			{
				if (parts.Length < 3)
				{
					context.Send($"Usage: createcopy {objectType} _templateId_ _id_");
					return false;
				}

				newId = parts[2];
			}

			if (!string.IsNullOrEmpty(newId) && storage.FindById(context, newId) != null)
			{
				context.Send($"Id {newId} is used already.");
				return false;
			}

			var asCloneable = obj as ICloneable;
			if (asCloneable == null)
			{
				context.Send($"Object of type {objectType} can't be copied.");
				return false;
			}

			var newObject = asCloneable.Clone();
			if (!string.IsNullOrEmpty(newId))
			{
				var editor = ClassEditor.GetEditor(storage.ObjectType);
				var property = editor.FindByName("Id");
				if (property == null)
				{
					context.Send($"Object of type {objectType} lacks 'Id' property.");
					return false;
				}

				if (!property.SetStringValue(context, newObject, new[] { newId }))
				{
					return false;
				}
			}

			context.SaveObject(newObject);
			context.Send($"Created new object {newObject.GetStringId()} of type {objectType}");

			if (storage.CanSpawn)
			{
				Spawn.Execute(context, $"item {newId}");
			}

			return true;
		}
	}
}
