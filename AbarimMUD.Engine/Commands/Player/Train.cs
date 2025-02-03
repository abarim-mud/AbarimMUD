using AbarimMUD.Data;
using System.Linq;
using System.Text;

namespace AbarimMUD.Commands.Player
{
	public class Train: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				context.Send("You can't train.");
				return false;
			}

			var spentSkillPoints = character.SpentSkillPointsCount;
			if (spentSkillPoints >= SkillCostInfo.Storage.Count)
			{
				context.Send($"You can't train anymore.");
				return false;
			}

			var price = SkillCostInfo.GetSkillCostInfo(spentSkillPoints).Gold;

			var trainer = (from m in context.Room.Mobiles where m.Info.Guildmaster != null select m).FirstOrDefault();
			if (trainer == null)
			{
				context.Send("Noone can train you here.");
				return false;
			}

			var trainerContext = (ExecutionContext)trainer.Tag;

			var trainableSkills = (from s in Skill.Storage where s.Class.Id.EqualsToIgnoreCase(trainer.Info.Guildmaster.Id) select s).ToList();

			Skill skillToTrain = null;
			if (!string.IsNullOrEmpty(data))
			{
				skillToTrain = (from s in trainableSkills where s.Name.EqualsToIgnoreCase(data) select s).FirstOrDefault();
			}

			// Remove maxed out skills
			SkillValue skillValue = null;
			foreach(var pair in character.Skills)
			{
				if (skillToTrain != null && skillToTrain.Id == pair.Value.Skill.Id)
				{
					skillValue = pair.Value;
				}

				if (pair.Value.Level == SkillLevel.Master)
				{
					trainableSkills.Remove(pair.Value.Skill);
				}
			}

			if (string.IsNullOrEmpty(data))
			{
				if (trainableSkills.Count == 0)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} There's nothing I could teach you anymore.");
				}
				else
				{
					var sb = new StringBuilder();
					for(var i = 0; i < trainableSkills.Count; ++i)
					{
						sb.Append($"{trainableSkills[i].Name} ({trainableSkills[i].Cost} sp)");

						if (i < trainableSkills.Count - 1)
						{
							sb.Append(", ");
						}
					}
					var skillNames = (from s in trainableSkills select s.Name).ToList();
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} I could teach you one of the following: {sb.ToString()}. It would cost you {price} gold.");
				}
			} else
			{
				if (skillToTrain == null)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} I don't know '{data}'.");
					return false;
				}

				if (skillValue != null && skillValue.Level == SkillLevel.Master)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} You are master in '{data}' already.");
					return false;
				}

				var levelConstraint = 1;
				if(character.Class.Id.EqualsToIgnoreCase(trainer.Info.Guildmaster.Id))
				{
					// Skill of the primary class
					levelConstraint = Configuration.PrimarySkillsLevelsConstraints[(int)skillValue.Level + 1];
				} else
				{
					// Skill not of the primary class
					levelConstraint = Configuration.NonPrimarySkillsLevelsConstraints[(int)skillValue.Level + 1];
				}

				if (character.Level < levelConstraint)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} You need to have at least level {levelConstraint} to learn that.");
					return false;
				}

				if (character.SkillPoints < skillToTrain.Cost)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} Unfortunately you don't have enough skill points.");
					return false;
				}

				if (character.Gold < price)
				{
					Tell.Execute(trainerContext, $"{context.Creature.ShortDescription} Unfortunately you don't have enough gold.");
					return false;
				}

				character.Train(skillToTrain);
				character.Gold -= price;
				character.SkillPoints -= skillToTrain.Cost;

				character.Save();

				if (skillValue == null)
				{
					context.Send($"You learned new skill: {skillToTrain.Name}.");
				} else
				{
					context.Send($"You advanced {skillToTrain.Name} to the next level.");
				}
			}
			
			return true;
		}
	}
}
