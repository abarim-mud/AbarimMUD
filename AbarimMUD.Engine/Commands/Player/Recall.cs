using AbarimMUD.Data;

namespace AbarimMUD.Commands.Player
{
	public class Recall : PlayerCommand
	{
		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var ability = Ability.EnsureAbilityById("recall");
			var ap = new AbilityPower(ability);

			return context.UseAbility(ap, context.ShortDescription, null, null);
		}
	}
}
