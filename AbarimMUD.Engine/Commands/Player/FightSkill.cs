using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace AbarimMUD.Commands.Player
{
	public class FightSkill : PlayerCommand
	{
		private static readonly Regex SpellRegex = new Regex(@"'(.*)'", RegexOptions.Compiled);

		private void SendUsage(ExecutionContext context)
		{
			var sb = new StringBuilder();

			sb.Append("Usage: fightskill ");

			foreach (var pair in AllCommands)
			{
				if (pair.Key.EqualsToIgnoreCase("cast") || !pair.Value.CanFightskill)
				{
					continue;
				}

				var ability = context.Creature.Stats.GetAbilityById(pair.Key);
				if (ability == null)
				{
					continue;
				}

				sb.Append($"{pair.Key}|");
			}

			sb.Append("cast '_spellName'|off");

			context.Send(sb.ToString());
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("Only players can set the fightskill.");
				return false;
			}

			var parts = data.SplitByWhitespace(2);
			if (parts.Length == 0)
			{
				SendUsage(context);
				return false;
			}

			var isCast = parts[0].EqualsToIgnoreCase("cast");
			if ((!isCast && parts.Length > 1) ||
				(isCast && parts.Length != 2))
			{
				SendUsage(context);
				return false;
			}

			var cmd = parts[0].ToLower();
			AbilityPower ap = null;
			if (isCast)
			{
				// Parse the spell name
				var spellName = parts[1].ToLower();
				var match = SpellRegex.Match(spellName);
				if (!match.Success)
				{
					context.Send($"Can't parse spell \"{spellName}\".");
					return false;
				}

				spellName = match.Groups[1].Value;
				ap = context.EnsureSpellByName(spellName);
				if (ap == null)
				{
					return false;
				}

				if (!ap.Ability.IsFightSkill)
				{
					context.Send($"Spell \"{spellName}\" can't be set as the fight skill.");
					return false;
				}
			}
			else
			{
				ap = context.EnsurePhysicalByName(cmd);
				if (ap == null)
				{
					return false;
				}

				if (!ap.Ability.IsFightSkill)
				{
					context.Send($"Ability \"{cmd}\" can't be set as the fight skill.");
					return false;
				}
			}

			if (cmd.EqualsToIgnoreCase("off"))
			{
				character.FightSkill = null;
				character.Save();

				context.Send("Cleared the current fightskill.");
				return true;
			}

			character.FightSkill = ap.Ability;
			character.Save();
			context.Send($"fightskill was set to \"{ap.Ability.Name}\".");

			return true;
		}
	}
}
