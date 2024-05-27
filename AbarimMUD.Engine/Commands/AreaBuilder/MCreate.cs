using AbarimMUD.Data;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class MCreate : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Create new mobile
			var newMobile = new Mobile
			{
				Id = Database.NewMobileId,
				Name = "unset",
				ShortDescription = "Unset",
				LongDescription = "A mobile with 'unset' name is standing here.",
				Description = "Unset."
			};
			newMobile.InitializeLists();

			var area = context.CurrentRoom.Area;
			area.Mobiles.Add(newMobile);
			Database.Update(newMobile);

			context.SendTextLine(string.Format("New mobile info (#{0}) had been created for the area {1} (#{2})",
				newMobile.Id,
				context.CurrentRoom.Area.Name,
				context.CurrentRoom.Area.Id));

			new MSpawn().Execute(context, newMobile.Id.ToString());
		}
	}
}