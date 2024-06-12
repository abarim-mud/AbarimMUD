using AbarimMUD.Data;
using System.Collections.Generic;

namespace AbarimMUD.Commands.AreaBuilder
{
	public class MobileCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.SendTextLine("Usage: mobilecreate _newMobileId");
				return;
			}

			var id = data;
			var existing = Mobile.GetMobileById(id);
			if (existing != null)
			{
				context.SendTextLine($"Id {id} is used by {existing} already");
				return;
			}

			// Create new mobile
			var newMobile = new Mobile
			{
				Id = id,
				Keywords = new HashSet<string> { "unset" },
				ShortDescription = "Unset",
				Description = "Unset.",
				Race = Race.EnsureRaceById(Configuration.DefaultRace),
				Class = GameClass.EnsureClassById(Configuration.DefaultClass),
				Level = 1,
			};

			newMobile.Create();
			context.SendTextLine($"New mobile {newMobile} was created");

			MobileSpawn.Execute(context, newMobile.Id);
		}
	}
}
