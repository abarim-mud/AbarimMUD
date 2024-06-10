using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.AreaBuilder
{
	public sealed class RoomSaveResets : AreaBuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Remove existing room resets
			var area = context.CurrentArea;
			var toDelete = (from r in area.Resets where r.Id2 == context.CurrentRoom.Id select r).ToList();

			foreach(var d in toDelete)
			{
				area.Resets.Remove(d);
			}

			// Add new
			foreach(var mobile in context.CurrentRoom.Mobiles)
			{
				area.Resets.Add(new AreaReset
				{
					ResetType = AreaResetType.NPC,
					Id1 = mobile.Info.Id,
					Id2 = context.CurrentRoom.Id
				});
			};

			area.Save();

			context.SendTextLine($"{toDelete.Count} mobile resets were removed. {context.CurrentRoom.Mobiles.Count} mobile resets were added.");
		}
	}
}
