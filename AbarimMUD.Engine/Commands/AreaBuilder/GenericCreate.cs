using AbarimMUD.Commands.AreaBuilder.OLCUtils;
using AbarimMUD.Data;
using System;

namespace AbarimMUD.Commands.AreaBuilder
{
	public abstract class GenericCreate<T> : AreaBuilderCommand where T : IStoredInFile, new()
	{
		private readonly Func<string, T> _itemGetter;

		protected GenericCreate(Func<string, T> itemGetter)
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
				context.Send($"Usage: {commandName} _{typeName}Id_");
				return;
			}

			var id = parts[0];
			var existing = _itemGetter(id);
			if (existing != null)
			{
				context.Send($"Id {id} is used by {existing} already");
				return;
			}

			// Create new mobile
			var newEntity = new T();
			context.SetStringId(newEntity, id);

			PreCreate(context, newEntity);

			newEntity.Create();

			PostCreate(context, newEntity);

			context.Send($"New {typeName} {id} was created.");
		}

		protected abstract void PreCreate(ExecutionContext context, T newEntity);

		protected abstract void PostCreate(ExecutionContext context, T newEntity);
	}
}
