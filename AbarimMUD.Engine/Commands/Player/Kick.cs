using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Player
{
	public class Kick : PlayerCommand
	{
		public override bool CanFightskill => true;

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			// Check the player has the skill
			var ability = context.EnsurePhysicalByName("kick");
			if (ability == null)
			{
				return false;
			}

			return context.UseAbility(ability, data,
				() =>
				{
					var baseDamage = ability.Power;
					var damageRange = new ValueRange(baseDamage, baseDamage + 4);
					return damageRange.Random();
				}, null);
		}

		public override int CalculateLagInMs(ExecutionContext context, string data = "") => Ability.Kick.PhysicalCommandLagInMs;
	}
}
