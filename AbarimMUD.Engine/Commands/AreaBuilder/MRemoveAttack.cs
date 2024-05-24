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

			var area = context.CurrentArea;
			var mobileInfo = area.GetMobileById(id);
			if (mobileInfo == null)
			{
				context.Send($"Unable to find mobile info with id {id}");
				return;
			}

			if (attackId >= mobileInfo.Attacks.Count)
			{
				context.Send($"Mobile with id {id} doesnt have attack with id {attackId}. It has only {mobileInfo.Attacks.Count} attacks");
				return;
			}

			var attack = mobileInfo.Attacks[attackId];
			mobileInfo.Attacks.RemoveAt(attackId);
			Database.Areas.Update(area);
			context.Send($"Removed #{attackId} attack ({attack}) from {mobileInfo.ShortDescription} (#{mobileInfo.Id})");
		}
	}
}
