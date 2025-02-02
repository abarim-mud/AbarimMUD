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

			var trainer = (from m in context.Room.Mobiles where m.Info.Guildmaster != null select m).FirstOrDefault();
			if (trainer == null)
			{
				context.Send("Noone can train you here.");
				return false;
			}

			var trainableSkills = (from s in Skill.Storage where s.Class.Id == trainer.Info.Guildmaster.Id select s).ToList();
			
			// Remove maxed out skills
			foreach(var pair in asCharacter.Skills)
			{
				if (pair.Value.Level == SkillLevel.Master)
				{
					trainableSkills.Remove(pair.Value.Skill);
				}
			}

			if (trainableSkills == null)
			{
				Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} There's nothing I could teach you anymore.");
			}
			else
			{
				var skillNames = (from s in trainableSkills select s.Name).ToList();
				var price = SkillCostInfo.GetSkillCostInfo(spentSkillPoints).Gold;
				Tell.Execute((ExecutionContext)trainer.Tag, $"{context.Creature.ShortDescription} I could teach you one of the following: {string.Join(", ", skillNames)}. It would cost you {price} gold.");
			}
			
			return true;
		}
	}
}
