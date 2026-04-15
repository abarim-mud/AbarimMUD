using AbarimMUD.Commands.Builder;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using NLog.Targets;
using System;
using System.Collections.Generic;
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
			var ap = context.EnsureAbilityByName(spellName);
			if (ap == null)
			{
				context.Send($"You don't know spell '{spellName}'.");
				return false;
			}

			var targetName = match.Groups[2].Value;
			ExecutionContext target = null;
			if (!string.IsNullOrWhiteSpace(targetName))
			{
				target = context.Room.Find(targetName);
				if (target == null)
				{
					context.Send($"There isnt '{targetName}' in this room");
					return false;
				}
			}

			var ability = ap.Ability;
			if (context.Creature.State.Moves < ability.MovesCost)
			{
				context.Send("You are too tired.");
				return false;
			}

			if (context.Creature.State.Mana < ability.ManaCost)
			{
				context.Send("You don't have enough mana to cast that spell.");
				return false;
			}

			context.Creature.State.Mana -= ability.ManaCost;
			context.Creature.State.Moves -= ability.MovesCost;

			context.Send($"You cast '{ability.Name}'.");

			var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			{
				{ "user.name", context.ShortDescription },
			};

			if (target != null)
			{
				variables["target.name"] = target.ShortDescription;
			}

			if (!string.IsNullOrEmpty(ability.MessageHitUser))
			{
				var message = ability.MessageHitUser.FormatMessage(variables);
				context.Send(message);
			}

			if (!string.IsNullOrEmpty(ability.MessageHitRoom))
			{
				var message = ability.MessageHitRoom.FormatMessage(variables);
				context.SendRoomExceptMe(message);
			}

			if (ability.Affects != null)
			{
				foreach (var pair in ability.Affects)
				{
					var affect = pair.Value;
					context.Creature.AddTemporaryAffect(affect.AffectSlotName, ability.Name, affect.Type, affect.Value ?? ap.Power, affect.DurationInSeconds.Value);
				}
			}

			if (ability.InstantEffects != null)
			{
				foreach (var instantEffect in ability.InstantEffects)
				{
					var power = instantEffect.Power.Random();
					switch (instantEffect.Type)
					{
						case InstantEffectType.Heal:
							context.Creature.Heal(power, 0, 0);
							break;
					}
				}
			}

			return true;
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "")
		{
			return Configuration.CastLagInMs;
		}
	}
}
