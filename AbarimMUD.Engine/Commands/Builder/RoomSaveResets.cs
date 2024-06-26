using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands.Builder
{
	public sealed class RoomSaveResets : BuilderCommand
	{
		protected override void InternalExecute(ExecutionContext context, string data)
		{
			// Remove existing room resets
			var area = context.CurrentArea;
			var toDelete = (from r in area.MobileResets where r.RoomId == context.Room.Id select r).ToList();

			foreach(var d in toDelete)
			{
				area.MobileResets.Remove(d);
			}

			// Add new
			foreach(var mobile in context.Room.Mobiles)
			{
				area.MobileResets.Add(new AreaMobileReset(mobile.Info.Id, context.Room.Id));
			};

			area.Save();

			context.Send($"{toDelete.Count} mobile resets were removed. {context.Room.Mobiles.Count} mobile resets were added.");
		}
	}
}
