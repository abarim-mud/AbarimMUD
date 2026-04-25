using AbarimMUD.Data;
using System.Text.RegularExpressions;

namespace AbarimMUD.Commands.Player
{
	public class Cast : PlayerCommand
	{
		private static readonly Regex SpellRegex = new Regex(@"'(.*)'\s*(.*)?\s*", RegexOptions.Compiled);

		public override bool CanFightskill => true;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Parse the spell name
			var match = SpellRegex.Match(data);
			if (!match.Success)
			{
				context.Send("Usage: cast 'spell name' [target]");
				return false;
			}

			var spellName = match.Groups[1].Value;

			// Check the player has the skill
			var ap = context.EnsureSpellByName(spellName);
			if (ap == null)
			{
				return false;
			}

			var targetName = match.Groups[2].Value;

			return context.UseAbility(ap, targetName, null, null);
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "") => Configuration.CastLagInMs;
	}
}
