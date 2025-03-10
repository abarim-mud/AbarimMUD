using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Title : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: title <your_new_title>");
				return false;
			}

			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Titles could be set only for the characters.");
				return false;
			}

			character.Title = data;
			character.Save();

			context.Send($"You set title to '{data}'.");

			return true;
		}
	}
}
