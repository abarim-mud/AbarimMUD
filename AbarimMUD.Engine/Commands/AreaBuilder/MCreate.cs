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
				Id = Area.NextMobileId,
				Name = "unset",
				ShortDescription = "Unset",
				LongDescription = "A mobile with 'unset' name is standing here.",
				Description = "Unset."
			};

			var area = context.CurrentRoom.Area;
			area.Mobiles.Add(newMobile);
			area.Save();

			context.SendTextLine($"New mobile info (#{newMobile.Id}) had been created for the area '{area.Name}'");

			new MSpawn().Execute(context, newMobile.Id.ToString());
		}
	}
}