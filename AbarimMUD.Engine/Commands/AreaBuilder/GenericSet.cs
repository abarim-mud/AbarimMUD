using AbarimMUD.Commands.AreaBuilder.OLCUtils;
using AbarimMUD.Data;
using System;
using System.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class GenericSet<T> : AreaBuilderCommand where T : IEntity
	{
		private readonly Func<ExecutionContext, string, T> _itemGetter;

		public GenericSet(Func<ExecutionContext, string, T> itemGetter)
		{
			_itemGetter = itemGetter ?? throw new ArgumentNullException(nameof(itemGetter));
		}

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var commandName = GetType().Name.ToLower();
			var parts = data.SplitByWhitespace(3);

			var editor = ClassEditor.GetEditor<T>();
			var typeName = typeof(T).Name;
			if (parts.Length < 1)
			{
				var propsNames = string.Join("|", from p in editor.Records.Values select p.Name.ToLower());

				context.Send($"Usage: {commandName} {propsNames} _{typeName}Id_ _params_");
				return;
			}

			var record = editor.FindByName(parts[0]);
			if (record == null)
			{
				context.Send($"Unable to find property '{parts[0]}' for type {typeName}");
				return;
			}

			if (parts.Length < 3)
			{
				context.Send($"Usage: {commandName} {record.Name} _{typeName}Id_ _params_");
				return;
			}

			var item = _itemGetter(context, parts[1]);
			if (item == null)
			{
				return;
			}

			var s = parts[2];

			if (record.Type == typeof(string))
			{
				record.SetValue(item, s);
			} else if(record.Type == typeof(GameClass))
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

				record.SetValue(item, cls);
			} else
			{
				context.Send($"Setting propertes of type '{record.Type.Name}' isn't implemented.");
				return;
			}

			item.Save();
			context.Send($"Changed {item.Id}'s {record.Name} to '{s}'");
		}
	}
}
