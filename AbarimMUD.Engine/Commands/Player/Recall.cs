using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Recall : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			if (context.Creature.State.Mana < Configuration.RecallManaCost)
			{
				context.Send($"You don't have enough mana. {Configuration.RecallManaCost} mana is required.");
				return false;
			}

			context.Room = Room.EnsureRoomById(Configuration.StartRoomId);
			context.Send("You recalled.");

			context.State.Mana -= Configuration.RecallManaCost;

			Look.Execute(context);

			return true;
		}
	}
}
