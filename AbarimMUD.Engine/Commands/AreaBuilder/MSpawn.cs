using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MSpawn : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			var id = -1;
			if (string.IsNullOrEmpty(data) || !int.TryParse(data, out id))
			{
				context.Send("Usage: mspawn _id_");
				return;
			}

			var mobileInfo = context.CurrentArea.GetMobileById(id);
			if (mobileInfo == null)
			{
				context.Send(string.Format("Unable to find mobile info with id {0}", id));
				return;
			}

			// Create new mobile
			var newMobile = new MobileInstance(mobileInfo)
			{
				Room = context.CurrentRoom
			};

			foreach (var ctx in context.AllInRoom())
			{
				if (ctx.IsStaff)
				{
					ctx.SendTextLine(string.Format("{0} (#{1}) appears in a puff of smoke.",
						mobileInfo.Name,
						mobileInfo.Id));
				}
				else
				{
					ctx.SendTextLine(string.Format("{0} appears in a puff of smoke.",
						mobileInfo.Name));
				}
			}
		}
	}
}