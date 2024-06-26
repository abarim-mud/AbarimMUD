﻿using AbarimMUD.Data;
using AbarimMUD.Storage;

namespace AbarimMUD.Commands.Player
{
	public class Score : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			context.Send($"You are {context.ShortDescription}, {context.Creature.Class.Name} of level {context.Creature.Level}.");
			var asCharacter = context.Creature as Character;
			if (asCharacter != null)
			{
				if (asCharacter.Level < Configuration.MaximumLevel)
				{
					var nextLevelInfo = LevelInfo.GetLevelInfo(asCharacter.Level + 1);
					context.Send($"Experience: {asCharacter.Experience.FormatBigNumber()}/{nextLevelInfo.Experience.FormatBigNumber()}");
				}
				else
				{
					context.Send($"Experience: {asCharacter.Experience.FormatBigNumber()}");
				}

				context.Send($"Gold: {asCharacter.Wealth.FormatBigNumber()}");

				if (string.IsNullOrEmpty(asCharacter.Autoskill))
				{
					context.Send("You didnt set the autoskill.");
				}
				else
				{
					context.Send($"You current autoskill is {asCharacter.Autoskill}.");
				}
			}

			var stats = context.Creature.Stats;
			var state = context.Creature.State;

			var regen = stats.GetHitpointsRegen(context.IsFighting);
			context.Send($"Hitpoints: {state.Hitpoints}/{stats.MaxHitpoints} + {regen}");

			regen = stats.GetManaRegen(context.IsFighting);
			context.Send($"Mana: {state.Mana}/{stats.MaxMana} + {regen}");

			regen = stats.GetMovesRegen(context.IsFighting);
			context.Send($"Moves: {state.Moves}/{stats.MaxMoves} + {regen}");

			context.Send("Armor: " + stats.Armor);
			for (var i = 0; i < stats.Attacks.Count; i++)
			{
				var attack = stats.Attacks[i];
				context.Send($"Attack #{i + 1}: {attack}");
			}

			if (stats.BackstabCount > 0)
			{
				if (stats.BackstabCount == 1)
				{
					context.Send($"You can do single backstab with multiplier equal to {stats.BackstabMultiplier}.");
				} else
				{
					context.Send($"You can do {stats.BackstabCount} backstabs with multiplier equal to {stats.BackstabMultiplier}.");
				}
			}

			return true;
		}
	}
}
