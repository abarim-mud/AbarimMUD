using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Builder
{
	public class CreateCopy : BuilderCommand
	{
		private bool CreateRoom(ExecutionContext context)
		{
			if (context.Creature.Room.Area == null)
			{
				context.Send($"Can't create a room for the default area.");
				return false;
			}

			var area = context.CurrentArea;
			var room = context.Room;
			var newId = area.NextRoomId;

			// Create new room
			var newRoom = new Room
			{
				Id = newId,
				Name = room.Name,
				Description = room.Description,
				SectorType = room.SectorType,
				ExtraKeyword = room.ExtraKeyword,
				ExtraDescription = room.ExtraDescription
			};

			if (room.Flags != null)
			{
				foreach (var flag in room.Flags)
				{
					newRoom.Flags.Add(flag);
				}
			}

			area.Rooms.Add(newRoom);
			area.Save();

			context.Send($"New room (#{newRoom.Id}) had been created for the area '{context.Room.Area.Name}'.");
			Goto.Execute(context, newRoom.Id.ToString());

			return true;
		}

		private bool CreateMobile(ExecutionContext context, int mobileId)
		{
			if (context.Creature.Room.Area == null)
			{
				context.Send($"Can't create a room for the default area.");
				return false;
			}

			var area = context.CurrentArea;
			var mobile = area.Mobiles[mobileId];
			var newId = area.NextMobileId;

			var newMobile = mobile.CloneMobile();
			newMobile.Id = newId;
			area.Mobiles.Add(newMobile);

			area.Save();

			context.Send($"New mobile (#{newMobile.Id}) had been created for the area '{context.Room.Area.Name}'.");

			return true;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: createcopy room|mobile [_id_]");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			// Room creation doesnt require id
			if (objectType != "room")
			{
				if (parts.Length < 2)
				{
					if (objectType != "mobilespawn")
					{
						context.Send($"Usage: createcopy {objectType} _id_");
					}
					else
					{
						context.Send($"Usage: createcopy {objectType} _mobileId_");
					}

					return false;
				}
			}

			switch (objectType)
			{
				case "room":
					return CreateRoom(context);

				case "mobile":
					{
						int id;
						if (!context.EnsureInt(parts[1], out id))
						{
							return false;
						}

						return CreateMobile(context, id);
					}

				default:
					context.Send($"Unknown object type '{objectType}'.");
					break;
			}

			return false;
		}

	}
}
