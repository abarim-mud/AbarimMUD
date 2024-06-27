namespace AbarimMUD.Commands.Player
{
	public class Equipment : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var grid = context.Creature.BuildEquipmentDesc();
			if (grid == null)
			{
				context.Send("You aren't using any items");
			}
			else
			{
				context.Send("You are using:");
				context.Send(grid.ToString());
			}

			return true;
		}
	}
}
