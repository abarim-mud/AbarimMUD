using System.Linq;
using System.Text;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Player
{
	public class Abilities : PlayerCommand
	{
		private static string PowerToString(int power)
		{
			if (power == 0)
			{
				return string.Empty;
			}

			return power.ToString();
		}

		private static string CostToString(Ability ab)
		{
			if (ab.ManaCost != 0 && ab.MovesCost != 0)
			{
				return $"{ab.ManaCost} mana, {ab.MovesCost} moves";
			}
			else if (ab.ManaCost != 0)
			{
				return $"{ab.ManaCost} mana";
			}
			else if (ab.MovesCost != 0)
			{
				return $"{ab.MovesCost} moves";
			}

			return string.Empty;
		}

		private static string BuildDescription(Ability ab)
		{
			if (!string.IsNullOrEmpty(ab.Description))
			{
				return ab.Description;
			}

			var sb = new StringBuilder();

			var firstLine = true;

			if (ab.Affects != null)
			{
				foreach (var pair in ab.Affects)
				{
					if (!firstLine)
					{
						sb.AppendLine();
					}

					var affect = pair.Value;

					var power = affect.Value == null ? "_power_" : affect.Value.ToString();
					var minutes = (affect.DurationInSeconds.Value / 60.0f).FormatFloat();
					sb.Append($"Raises {pair.Key} by {power} for {minutes} minutes.");

					firstLine = false;
				}
			}

			return sb.ToString();
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				// Skills command is available only for characters
				return false;
			}

			context.Send("Physical:");

			var grid = new AsciiGrid();

			grid.SetHeader(0, "Name");
			grid.SetHeader(1, "Power");
			grid.SetHeader(2, "Cost");
			grid.SetHeader(3, "Description");

			var abilities = from s in context.Stats.Abilities where s.Ability.Type == AbilityType.Physical orderby s.Id select s;

			var i = 0;
			foreach (var ap in abilities)
			{
				var ability = ap.Ability;
				grid.SetValue(0, i, ability.Name);
				grid.SetValue(1, i, PowerToString(ap.Power));
				grid.SetValue(2, i, CostToString(ability));
				grid.SetValue(3, i, BuildDescription(ability));

				++i;
			}

			context.Send(grid.ToString());
			context.Send();

			context.Send("Spells:");

			grid = new AsciiGrid();

			grid.SetHeader(0, "Name");
			grid.SetHeader(1, "Power");
			grid.SetHeader(2, "Cost");
			grid.SetHeader(3, "Description");

			abilities = from s in context.Stats.Abilities where s.Ability.Type == AbilityType.Spell orderby s.Id select s;

			i = 0;
			foreach (var ap in abilities)
			{
				var ability = ap.Ability;
				grid.SetValue(0, i, ability.Name);
				grid.SetValue(1, i, PowerToString(ap.Power));
				grid.SetValue(2, i, CostToString(ability));
				grid.SetValue(3, i, BuildDescription(ability));

				++i;
			}

			context.Send(grid.ToString());
			context.Send();

			return true;
		}
	}
}
