﻿using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Builder
{
	public class Info : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 2)
			{
				context.Send($"Usage: info {OLCManager.KeysString} [_id_]");
				return false;
			}

			var key = parts[0].ToLower();
			var storage = context.EnsureStorage(key);
			if (storage == null)
			{
				return false;
			}

			var item = storage.FindById(context, parts[1]);
			if (item == null)
			{
				context.Send($"Unable to find item of type {key} by id '{parts[1]}'");
				return false;
			}

			var editor = ClassEditor.GetEditor(item.GetType());
			var infoDict = editor.BuildInfoDict(item);

			context.Send(infoDict.ToAsciiGridString());
			
			return true;
		}
	}
}
