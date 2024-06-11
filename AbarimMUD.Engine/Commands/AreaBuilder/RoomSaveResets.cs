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
			var toDelete = (from r in area.MobileResets where r.RoomId == context.CurrentRoom.Id select r).ToList();

			foreach(var d in toDelete)
			{
				area.MobileResets.Remove(d);
			}

			// Add new
			foreach(var mobile in context.CurrentRoom.Mobiles)
			{
				area.MobileResets.Add(new AreaMobileReset(mobile.Info.Id, context.CurrentRoom.Id));
			};

			area.Save();

			context.SendTextLine($"{toDelete.Count} mobile resets were removed. {context.CurrentRoom.Mobiles.Count} mobile resets were added.");
		}
	}
}
