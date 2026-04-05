using AbarimMUD.Data;
using System.Collections.Generic;

namespace AbarimMUD.Commands.Builder
{
	public class Delete : BuilderCommand
	{
		private bool DeleteRoom(ExecutionContext context, int roomId)
		{
			var room = context.EnsureRoomById(roomId);
			if (room == null)
			{
				return false;
			}

			if (room.Characters.Count > 0)
			{
				context.Send($"Can't delete the room with characters.");
				return false;
			}

			if (room.Mobiles.Count > 0)
			{
				context.Send($"Can't delete the room with mobiles.");
				return false;
			}

			var changedAreas = new HashSet<string>();

			// Delete all exits to that room
			foreach (var a in Area.Storage)
			{
				foreach (var r in a.Rooms)
				{
					if (r.Id == roomId)
					{
						continue;
					}

					var toDelete = new List<Direction>();
					foreach (var exit in r.Exits)
					{
						if (exit.Value.TargetRoom.Id == roomId)
						{
							toDelete.Add(exit.Key);
						}
					}

					foreach (var td in toDelete)
					{
						r.Exits.Remove(td);
						context.Send($"Removed exit from {r} to {room}");
						changedAreas.Add(a.Id);
					}
				}
			}

			// Finally delete the room
			var area = room.Area;
			changedAreas.Add(area.Id);
			room.Area.Rooms.Remove(room);

			context.Send($"Room {room} had been deleted from the area '{area.Name}'.");

			// Save changed areas
			foreach (var areaId in changedAreas)
			{
				area = Area.GetAreaByName(areaId);
				context.Send($"Saving area {area}...");
				area.Save();
			}

			return true;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 2)
			{
				context.Send($"Usage: delete room [_id_]");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			// Todo room creation doesnt require id
			if (objectType != "room")
			{
				context.Send($"Usage: delete room [_id_]");
				return false;
			}

			int roomId;
			if (!context.EnsureInt(parts[1], out roomId))
			{
				return false;
			}

			switch (objectType)
			{
				case "room":
					return DeleteRoom(context, roomId);
			}

			return true;
		}
	}
}
