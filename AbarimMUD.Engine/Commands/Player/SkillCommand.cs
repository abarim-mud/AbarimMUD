using AbarimMUD.Data;
using AbarimMUD.Utils;
using System.Linq;

namespace AbarimMUD.Commands.Player
{
	public class SkillCommand : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var character = context.Creature as Character;
			if (character == null)
			{
				// Skills command is available only for characters
				return false;
			}

			context.Send("Skills:");

			var skillsGrid = new AsciiGrid();

			skillsGrid.SetHeader(0, "Name");
			skillsGrid.SetHeader(1, "Learned");
			skillsGrid.SetHeader(2, "Guild");
			skillsGrid.SetHeader(3, "Next Train Level");

			var orderedSkills = from s in Skill.Storage orderby s.Class.Id, s.Name select s;

			var i = 0;
			foreach (var skill in orderedSkills)
			{
				skillsGrid.SetValue(0, i, skill.Name);

				SkillValue skillValue = null;
				character.Skills.TryGetValue(skill.Id, out skillValue);

				var lev = skillValue != null ? skillValue.Level : 0;
				var s = $"{lev}/{skill.TotalLevels}";
				skillsGrid.SetValue(1, i, s);
				skillsGrid.SetValue(2, i, skill.Class.Name);


				var levelConstraint = Train.CalculateLevelConstraint(character, skill);
				s = levelConstraint != null ? levelConstraint.ToString() : string.Empty;
				skillsGrid.SetValue(3, i, s);

				++i;
			}

			context.Send(skillsGrid.ToString());

			if (character.SkillPoints == 0)
			{
				context.Send("You have zero skill points.");

			}
			else if (character.SkillPoints == 1)
			{
				context.Send("You have 1 skill point.");
			}
			else
			{
				context.Send($"You have {character.SkillPoints} skill points.");
			}

			var spentSkillPoints = character.SpentSkillPointsCount;
			if (spentSkillPoints >= SkillCostInfo.Storage.Count)
			{
				context.Send($"You can't train anymore.");
			}
			else
			{
				context.Send($"Training next skill level would cost {SkillCostInfo.GetSkillCostInfo(spentSkillPoints + 1).Gold.FormatBigNumber()} gold.");
			}

			return true;
		}
	}
}
