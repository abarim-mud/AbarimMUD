using AbarimMUD.Data;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Commands.Player
{
	public class Score : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var asCharacter = context.Creature as Character;
			if (asCharacter != null)
			{
				context.Send($"You are {context.ShortDescription}, {asCharacter.ClassName} of level {context.Creature.Level}.");

				if (asCharacter.Level < Configuration.MaximumLevel)
				{
					var nextLevelInfo = LevelInfo.GetLevelInfo(asCharacter.Level + 1);
					context.Send($"Experience: {asCharacter.Experience.FormatBigNumber()}/{nextLevelInfo.Experience.FormatBigNumber()}");
				}
				else
				{
					context.Send($"Experience: {asCharacter.Experience.FormatBigNumber()}");
				}

				context.Send($"Gold: {asCharacter.Gold.FormatBigNumber()}");

				if (asCharacter.FightSkill == null)
				{
					context.Send("You didnt set the fightskill.");
				}
				else
				{
					context.Send($"You current fightskill is \"{asCharacter.FightSkill}\".");
				}

				if (!string.IsNullOrEmpty(asCharacter.StabWeapon))
				{
					var message = $"Stabweapon is set to '{asCharacter.StabWeapon}'. ";

					bool isWielded;
					var weapon = context.Creature.FindStabWeapon(asCharacter.StabWeapon, out isWielded);
					if (weapon == null)
					{
						message += "It doesn't correspond to any item.";
					}
					else
					{
						message += $"It corresponds to an item '{weapon.Name}'";

						if (isWielded)
						{
							message += " (wielded)";
						}
					}

					context.Send(message);
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

			if (stats.PhysicalResistance != 0)
			{
				context.Send("Physical Resistance: " + stats.PhysicalResistance);
			}

			if (stats.MagicResistance != 0)
			{
				context.Send("Magic Resistance: " + stats.MagicResistance);
			}

			if (stats.HolyResistance != Configuration.DefaultHolyResistance)
			{
				context.Send("Holy Resistance: " + stats.HolyResistance);
			}

			if (stats.FireResistance != Configuration.DefaultFireResistance)
			{
				context.Send("Fire Resistance: " + stats.FireResistance);
			}

			if (stats.ColdResistance != Configuration.DefaultColdResistance)
			{
				context.Send("Cold Resistance: " + stats.ColdResistance);
			}

			if (stats.ShockResistance != Configuration.DefaultShockResistance)
			{
				context.Send("Shock Resistance: " + stats.ShockResistance);
			}

			for (var i = 0; i < stats.Attacks.Count; i++)
			{
				var attack = stats.Attacks[i];
				context.Send($"Attack #{i + 1}: {attack}");
			}

			if (context.Creature.TemporaryAffects.Count > 0)
			{
				context.Send("Affects:");

				var now = DateTime.Now;
				foreach (var pair in context.Creature.TemporaryAffects)
				{
					var tempAffect = pair.Value;
					var left = tempAffect.DurationInSeconds - (int)(now - tempAffect.Started).TotalSeconds;

					context.Send($"{tempAffect.Name} ({left.FormatTime()})");
				}
			}

			return true;
		}
	}
}
