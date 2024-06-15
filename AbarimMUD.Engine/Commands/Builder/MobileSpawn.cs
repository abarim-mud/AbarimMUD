using AbarimMUD.Data;

namespace AbarimMUD.Commands.Builder
{
	public sealed class MobileSpawn : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			if (string.IsNullOrEmpty(data))
			{
				context.Send("Usage: mobilespawn _mobileId_");
				return;
			}

			int id;
			if (!context.EnsureInt(data, out id))
			{
				return;
			}

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
					ctx.Send(string.Format("{0} (#{1}) appears in a puff of smoke.",
						mobile.ShortDescription,
						mobile.Id));
				}
				else
				{
					ctx.Send(string.Format("{0} appears in a puff of smoke.",
						mobile.ShortDescription));
				}
			}
		}
	}
}