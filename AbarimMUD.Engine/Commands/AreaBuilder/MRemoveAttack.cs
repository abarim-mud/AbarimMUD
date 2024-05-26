namespace AbarimMUD.Commands.AreaBuilder
{
	public class MRemoveAttack : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			int id, attackId;
			if (!data.Parse(out id, out attackId))
			{
				context.Send("Usage: mremoveattack _mobileId_ _attackId_");
				return;
			}

			if (attackId < 0)
			{
				context.Send("attackId couldn't be negative");
				return;
			}

			var mobile = Database.GetMobileById(id);
			if (mobile == null)
			{
				context.Send($"Unable to find mobile info with id {id}");
				return;
			}

			if (attackId >= mobile.Attacks.Count)
			{
				context.Send($"Mobile with id {id} doesnt have attack with id {attackId}. It has only {mobile.Attacks.Count} attacks");
				return;
			}

			var attack = mobile.Attacks[attackId];
			mobile.Attacks.RemoveAt(attackId);
			Database.Update(mobile);
			context.Send($"Removed #{attackId} attack ({attack}) from {mobile.ShortDescription} (#{mobile.Id})");
		}
	}
}
