using AbarimMUD.Combat;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Builder
{
	public class Peace : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			var playerMessage = new StringBuilder();
			var roomMessage = new StringBuilder();
			foreach (var fight in Fight.AllFights)
			{
				if (!fight.Finished && fight.Room == context.CurrentRoom)
				{
					var side1Name = string.Join(", ", from s in fight.Participants where s.FightInfo.Side == FightSide.Side1 select s.ShortDescription);
					var side2Name = string.Join(", ", from s in fight.Participants where s.FightInfo.Side == FightSide.Side2 select s.ShortDescription);
					playerMessage.AppendLine($"You stopped fight between {side1Name} and {side2Name}.");
					roomMessage.AppendLine($"{context.ShortDescription} stopped fight between {side1Name} and {side2Name}.");
					fight.End();
				}
			}

			context.Send(playerMessage.ToString());
			foreach (var ctx in context.AllExceptMeInRoom())
			{
				ctx.Send(roomMessage.ToString());
			}
		}
	}
}
