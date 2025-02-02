using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class Train: PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var asCharacter = context.Creature as Character;
			if (asCharacter == null)
			{
				context.Send("You can't train.");
				return false;
			}

			var spentSkillPoints = asCharacter.SpentSkillPointsCount;
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

			var trainableSkills = (from s in Skill.Storage where s.Class.Id == trainer.Info.Guildmaster.Id select s).ToList();

			Skill skillToTrain = null;
			if (!string.IsNullOrEmpty(data))
			{
				skillToTrain = (from s in trainableSkills where s.Name.EqualsToIgnoreCase(data) select s).FirstOrDefault();
			}

			// Remove maxed out skills
			SkillValue skillValue = null;
			foreach(var pair in asCharacter.Skills)
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
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} There's nothing I could teach you anymore.");
				}
				else
				{
					var skillNames = (from s in trainableSkills select s.Name).ToList();
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} I could teach you one of the following: {string.Join(", ", skillNames)}. It would cost you {price} gold.");
				}
			} else
			{
				if (skillToTrain == null)
				{
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} I don't know '{data}'.");
					return false;
				}

				if (skillValue != null && skillValue.Level == SkillLevel.Master)
				{
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} You are master in '{data}' already.");
					return false;
				}

				if (asCharacter.SkillPoints <= 0)
				{
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} Unfortunately you don't have any skill points left.");
					return false;
				}

				if (asCharacter.Wealth < price)
				{
					Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} Unfortunately you don't have enough gold.");
					return false;
				}

				asCharacter.Train(skillToTrain);
				asCharacter.Wealth -= price;
				--asCharacter.SkillPoints;

				asCharacter.Save();

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
