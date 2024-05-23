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
		public static CRUD<Area> Areas => _dataContext.Areas;

		public static void Initialize()
		{
			_dataContext = new DataContext(Configuration.DataFolder, _dbLogger.Info);
		}

		public static void DisconnectRoom(Room room, Direction direction, bool updateArea = true)
		{
			var oppositeDir = direction.GetOppositeDirection();

			// Delete existing connections
			var existingConnection = (from e in room.Exits
									  where e.Direction == direction
									  select e).FirstOrDefault();

			if (existingConnection != null)
			{
				room.Exits.Remove(existingConnection);
				if (existingConnection.TargetRoom != null)
				{
					var oppositeConnection = (from e in existingConnection.TargetRoom.Exits
											  where e.TargetRoomId == room.Id && e.Direction == oppositeDir
											  select e).FirstOrDefault();

					if (oppositeConnection != null)
					{
						existingConnection.TargetRoom.Exits.Remove(oppositeConnection);
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
				TargetRoomId = targetRoom.Id,
				TargetRoom = targetRoom,
				Direction = direction
			};
			sourceRoom.Exits.Add(newConnection);

			var oppositeNewConnection = new RoomExit
			{
				TargetRoomId = sourceRoom.Id,
				TargetRoom = sourceRoom,
				Direction = direction.GetOppositeDirection()
			};
			targetRoom.Exits.Add(oppositeNewConnection);

			Areas.Update(sourceRoom.Area);
			if (sourceRoom.Area != targetRoom.Area)
			{
				Areas.Update(targetRoom.Area);
			}
		}

		public static int CalculateCharactersAmount() => Characters.Count;
	}
}
