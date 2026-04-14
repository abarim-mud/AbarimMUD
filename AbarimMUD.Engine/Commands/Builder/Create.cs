using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands.Builder
{
	public class Create : BuilderCommand
	{
		private bool CreateArea(ExecutionContext context, string id)
		{
			var existingArea = Area.GetAreaByName(id);
			if (existingArea != null)
			{
				context.Send($"Area with id '{id}' already exists.");
				return false;
			}

			// Determine max start id
			var maxId = 0;
			foreach (var a in Area.Storage.All)
			{
				if (a.StartId + a.IdCount > maxId)
				{
					maxId = a.StartId + a.IdCount;
				}
			}

			var character = (Character)context.Creature;
			var newArea = new Area
			{
				Id = id,
				Name = "New Area",
				StartId = maxId,
				OwnerName = character.Name,
				Builders = character.Name,
				Owner = character
			};

			var firstRoom = new Room
			{
				Id = newArea.StartId,
				Name = "First Room"
			};

			newArea.Rooms.Add(firstRoom);

			Area.Storage.Save(newArea);

			context.Send($"Created area '{id}'. Room id range: {maxId}-{maxId + newArea.IdCount}");

			Goto.Execute(context, firstRoom.Id.ToString());

			return true;
		}

		private bool CreateRoom(ExecutionContext context)
		{
			if (context.Creature.Room.Area == null)
			{
				context.Send($"Can't create a room for the default area.");
				return false;
			}

			var area = context.CurrentArea;
			var newId = area.NextRoomId;

			// Create new room
			var newRoom = new Room
			{
				Id = newId,
				Name = "New Room"
			};

			area.Rooms.Add(newRoom);
			area.Save();

			context.Send($"New room (#{newRoom.Id}) had been created for the area '{context.Room.Area.Name}'.");
			Goto.Execute(context, newRoom.Id.ToString());

			return true;
		}

		private bool CreateMobile(ExecutionContext context)
		{
			if (context.Creature.Room.Area == null)
			{
				context.Send($"Can't create a room for the default area.");
				return false;
			}

			var area = context.CurrentArea;
			var newId = area.NextMobileId;

			var newMobile = new Mobile
			{
				Id = newId,
				Sex = Sex.Male,
				Level = 1,
				ShortDescription = "short",
				LongDescription = "long",
				Description = "description"
			};

			var attack = new Attack(AttackType.Hit, 0, 5, 10);
			newMobile.Attacks = new Attack[]
			{
				attack
			};

			area.Mobiles.Add(newMobile);

			area.Save();

			context.Send($"New mobile (#{newMobile.Id}) had been created for the area '{context.Room.Area.Name}'.");

			return true;
		}

		private bool CreateMobileSpawn(ExecutionContext context, int mobileId)
		{
			var mobile = context.EnsureMobileById(mobileId);

			var newMobileSpawn = new MobileSpawn
			{
				Mobile = mobile,
			};

			context.Room.MobileSpawns.Add(newMobileSpawn);

			context.Room.Area.Save();

			context.Send($"Created new mobile spawn #{mobile.Id} in the room #{context.Room.Id}.");

			return true;
		}

		protected override bool InternalExecute(ExecutionContext context, string data)
		{
			var parts = data.SplitByWhitespace(4);
			if (parts.Length < 1)
			{
				context.Send($"Usage: create area|room|mobile|mobilespawn [_id_]");
				return false;
			}

			var objectType = parts[0].ToLower();
			var storage = context.EnsureStorage(objectType);
			if (storage == null)
			{
				return false;
			}

			// Todo room/mobile creation doesnt require id
			if (objectType != "room" && objectType != "mobile")
			{
				if (parts.Length < 2)
				{
					if (objectType != "mobilespawn")
					{
						context.Send($"Usage: create {objectType} _id_");
					}
					else
					{
						context.Send($"Usage: create {objectType} _mobileId_");
					}

					return false;
				}
			}

			switch (objectType)
			{
				case "area":
					return CreateArea(context, parts[1]);

				case "room":
					return CreateRoom(context);

				case "mobile":
					return CreateMobile(context);

				case "mobilespawn":
					{
						int id;
						if (!context.EnsureInt(parts[1], out id))
						{
							return false;
						}

						return CreateMobileSpawn(context, id);
					}
				default:
					context.Send($"Unknown object type '{objectType}'.");
					break;
			}

			return false;
		}
	}
}
