using System.Linq;
using AbarimMUD.Data;
using NLog;
using AbarimMUD.Storage;

namespace AbarimMUD
{
	public static class Database
	{
		private static Logger _dbLogger = LogManager.GetLogger("DB");
		private static DataContext _dataContext;

		public static CRUD<Account> Accounts => _dataContext.Accounts;
		public static Characters Characters => _dataContext.Characters;
		public static Areas Areas => _dataContext.Areas;

		public static int NewRoomId => Areas.NewRoomId;
		public static int NewMobileId => Areas.NewMobileId;
		public static int NewObjectId => Areas.NewObjectId;

		public static void Initialize()
		{
			_dataContext = new DataContext(Configuration.DataFolder, _dbLogger.Info);
		}

		public static void DisconnectRoom(Room room, Direction direction, bool updateArea = true)
		{
			var oppositeDir = direction.GetOppositeDirection();

			// Get the connection
			RoomExit existingConnection;
			if (!room.Exits.TryGetValue(direction, out existingConnection))
			{
				return;
			}

			if (existingConnection != null)
			{
				room.Exits.Remove(direction);
				if (existingConnection.TargetRoom != null)
				{
					RoomExit oppositeConnection;
					if (existingConnection.TargetRoom.Exits.TryGetValue(oppositeDir, out oppositeConnection) &&
						oppositeConnection.TargetRoom == room)
					{
						existingConnection.TargetRoom.Exits.Remove(oppositeDir);
					}
				}

				if (updateArea)
				{
					Areas.Update(room.Area);
				}
			}
		}

		public static void ConnectRoom(Room sourceRoom, Room targetRoom, Direction direction)
		{
			// Delete existing connections
			DisconnectRoom(sourceRoom, direction, false);

			// Create new ones
			var newConnection = new RoomExit
			{
				TargetRoom = targetRoom,
				Direction = direction
			};

			sourceRoom.Exits[direction] = newConnection;

			var oppositeNewConnection = new RoomExit
			{
				TargetRoom = sourceRoom,
				Direction = direction.GetOppositeDirection()
			};
			targetRoom.Exits[oppositeNewConnection.Direction] = oppositeNewConnection;

			Areas.Update(sourceRoom.Area);
			if (sourceRoom.Area != targetRoom.Area)
			{
				Areas.Update(targetRoom.Area);
			}
		}

		public static int CalculateCharactersAmount() => Characters.Count;

		public static Room GetRoomById(int id) => Areas.GetRoomById(id);
		public static Room EnsureRoomById(int id) => Areas.EnsureRoomById(id);
		public static Mobile GetMobileById(int id) => Areas.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Areas.EnsureMobileById(id);
		public static GameObject GetObjectById(int id) => Areas.GetObjectById(id);
		public static GameObject EnsureObjectById(int id) => Areas.EnsureObjectById(id);

		public static void Update(AreaEntity entity)
		{
			Areas.Update(entity.Area);
		}
	}
}
