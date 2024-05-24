﻿using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Create new mobile
			var newMobileInfo = new Mobile
			{
				Name = "unset",
				ShortDescription = "Unset",
				LongDescription = "A mobile with 'unset' name is standing here.",
				Description = "Unset."
			};

			var area = context.CurrentRoom.Area;
			area.Mobiles.Add(newMobileInfo);
			Database.Areas.Update(context.CurrentRoom.Area);

			context.SendTextLine(string.Format("New mobile info (#{0}) had been created for the area {1} (#{2})",
				newMobileInfo.Id,
				context.CurrentRoom.Area.Name,
				context.CurrentRoom.Area.Id));

			new MSpawn().Execute(context, newMobileInfo.Id.ToString());
		}
	}
}