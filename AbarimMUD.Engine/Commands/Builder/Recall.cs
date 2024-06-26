using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public class Recall: BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			context.Room = Room.EnsureRoomById(Configuration.StartRoomId);
			context.Send("You recalled.");

			Look.Execute(context);
		}
	}
}
