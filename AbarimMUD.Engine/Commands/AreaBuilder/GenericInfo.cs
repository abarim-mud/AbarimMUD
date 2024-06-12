using AbarimMUD.Utils;

namespace AbarimMUD.Commands.AreaBuilder
{
	public abstract class GenericInfo<T> : AreaBuilderCommand
	{
		protected abstract T GetById(ExecutionContext context, string id);

		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace();
			if (parts.Length < 1)
			{
				var name = GetType().Name.ToLower();
				var itemName = typeof(T).Name.ToLower();
				context.Send($"Usage: {name} _{itemName}Id_");
				return;
			}

			var item = GetById(context, parts[0]);
			if (item == null)
			{
				return;
			}

			context.SendTextLine(item.BuildInfoDict().ToAsciiGridString());
		}
	}
}
