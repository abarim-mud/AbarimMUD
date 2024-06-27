using AbarimMUD.Commands.Builder.OLCUtils;

namespace AbarimMUD.Commands.Builder
{
	public class Create: BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: create {OLCManager.KeysString} [_id_]");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			/*			// Create new mobile
						var newEntity = new T();
						context.SetStringId(newEntity, id);

						PreCreate(context, newEntity);

						newEntity.Create();

						PostCreate(context, newEntity);

						context.Send($"New {typeName} {id} was created.");*/


			return true;
		}
	}
}
