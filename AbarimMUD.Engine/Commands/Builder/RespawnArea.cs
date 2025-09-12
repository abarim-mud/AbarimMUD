namespace AbarimMUD.Commands.Builder
{
	public class RespawnArea : BuilderCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			Server.Instance.SpawnArea(context.Room.Area, context.Send);

			return true;
		}
	}
}
