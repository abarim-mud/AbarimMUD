using AbarimMUD.Commands.Builder.OLCUtils;
using AbarimMUD.Data;

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

			// Determine new room id
			var area = context.CurrentArea;
			var newId = area.StartId;
			foreach (var r in area.Rooms)
			{
				if (r.Id > newId)
				{
					newId = r.Id;
				}
			}

			++newId;

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

		private bool CreateMobile(ExecutionContext context, string id)
		{
			var newMobile = new Mobile
			{
				Id = id,
				Sex = Sex.Male,
				Level = 1,
				ShortDescription = id,
				LongDescription = id,
				Description = id
			};

			newMobile.Keywords.Add(id);

			var attack = new Attack(AttackType.Hit, 0, 5, 10);
			newMobile.Attacks = new Attack[]
			{
				attack
			};

			Mobile.Storage.Save(newMobile);

			context.Send($"Created new mobile (#{newMobile.Id}).");

			return true;
		}

		private bool CreateMobileSpawn(ExecutionContext context, string mobileId)
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
				context.Send($"Usage: create {OLCManager.KeysString} [_id_]");
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
				if (parts.Length < 2)
				{
					if (objectType != "mobilespawn")
					{
						context.Send($"Usage: create {objectType} _id_");
					} else
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
					return CreateMobile(context, parts[1]);

				case "mobilespawn":
					return CreateMobileSpawn(context, parts[1]);
			}

			/*			// Create new mobile
						var newEntity = new T();
						context.SetStringId(newEntity, id);

						PreCreate(context, newEntity);

						newEntity.Create();

						PostCreate(context, newEntity);

						context.Send($"New {typeName} {id} was created.");*/


			return true;
		}
	}
}
