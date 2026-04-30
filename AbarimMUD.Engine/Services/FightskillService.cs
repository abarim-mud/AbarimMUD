using AbarimMUD.Commands;
using AbarimMUD.Data;
using System;

namespace AbarimMUD.Services
{
	/// <summary>
	/// Service that handles characters' fightskills, i.e. the ability to automatically perform a skill when fighting, if certain conditions are met.
	/// </summary>
	internal class FightskillService : BaseService
	{
		protected override void InternalUpdate(TimeSpan elapsed)
		{
			// Process characters
			foreach (var character in Character.ActiveCharacters)
			{
				// Autoskill
				var ctx = character.GetContext();
				if (ctx.IsFighting && !ctx.WaitingCommandLag() && character.FightSkill != null)
				{
					var ab = character.FightSkill;
					if (ab.ManaCost < ctx.State.Mana && ab.MovesCost < ctx.State.Moves)
					{
						BaseCommand command = null;
						var data = string.Empty;
						switch (ab.Type)
						{
							case AbilityType.Physical:
								command = BaseCommand.FindCommand(ab.Name);
								break;
							case AbilityType.Spell:
								command = BaseCommand.Cast;
								data = $"'{ab.Name}'";
								break;
						}

						if (command != null)
						{
							ctx.SendInfoMessage($"Performing autoskill \"{character.FightSkill}\"");
							if (!command.Execute(ctx, data))
							{
								ctx.ParseAndExecute("autoskill off");
							}
						}

					}
				}
			}
		}
	}
}
