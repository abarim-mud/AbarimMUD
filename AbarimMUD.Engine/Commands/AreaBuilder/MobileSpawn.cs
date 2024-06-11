using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MobileSpawn : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: mobilespawn _mobileId_");
				return;
			}

			var id = data;
			var mobile = Mobile.GetMobileById(id);
			if (mobile == null)
			{
				context.Send(string.Format("Unable to find mobile info with vnum {0}", id));
				return;
			}

			// Create new mobile
			var newMobile = new MobileInstance(mobile)
			{
				Room = context.CurrentRoom
			};

			foreach (var ctx in context.AllInRoom())
			{
				if (ctx.IsStaff)
				{
					ctx.SendTextLine(string.Format("{0} (#{1}) appears in a puff of smoke.",
						mobile.Name,
						mobile.Id));
				}
				else
				{
					ctx.SendTextLine(string.Format("{0} appears in a puff of smoke.",
						mobile.Name));
				}
			}
		}
	}
}