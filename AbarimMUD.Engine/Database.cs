using System.Linq;
using System.Collections.Concurrent;
using AbarimMUD.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using AbarimMUD.Utils;
using System.Collections;
using NLog;

namespace AbarimMUD
{
	public static class Database
	{
		private class CRUD<T> : IEnumerable<T> where T : Entity
		{
			private readonly Func<DataContext, DbSet<T>> _tableGetter;
			private readonly ConcurrentDictionary<int, T> _cache = new ConcurrentDictionary<int, T>();

			public CRUD(Func<DataContext, DbSet<T>> tableGetter)
			{
				_tableGetter = tableGetter ?? throw new ArgumentNullException(nameof(tableGetter));
			}

			public void Add(T entity)
			{
				_cache[entity.Id] = entity;
			}

			public T GetById(int id)
			{
				T result;

				_cache.TryGetValue(id, out result);

				return result;
			}

			public void Create(T entity)
			{
				using (var db = new DataContext())
				{
					var table = _tableGetter(db);
					table.Add(entity);
					db.SaveChanges();
				}

				Add(entity);
			}

			public IEnumerator<T> GetEnumerator() => _cache.Values.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => _cache.Values.GetEnumerator();
		}

		private static Logger _dbLogger = LogManager.GetLogger("DB");
		private static CRUD<Area> _areas = new CRUD<Area>(db => db.Areas);
		private static CRUD<Room> _rooms = new CRUD<Room>(db => db.Rooms);
		private static CRUD<Mobile> _mobiles = new CRUD<Mobile>(db => db.Mobiles);
		private static CRUD<Account> _accounts = new CRUD<Account>(db => db.Accounts);
		private static ConcurrentDictionary<string, Account> _accountsByName = new ConcurrentDictionary<string, Account>();
		private static ConcurrentDictionary<string, Character> _charactersByName = new ConcurrentDictionary<string, Character>();

		public static void Initialize()
		{
			DataContext.LogOutput = msg => _dbLogger.Info(msg);

			using (var db = new DataContext())
			{
				// Load area data
				var areas = (from a in db.Areas select a).ToList();

				// Fetching this content will make areas fill their lists automatically
				var rooms = db.Rooms.ToList();
				var mobiles = db.Mobiles.ToList();
				var objects = db.Objects.ToList();
				var resets = db.AreaResets.ToList();
				var roomsExits = db.RoomsExits.ToList();

				foreach (var a in areas)
				{
					_areas.Add(a);

					foreach (var r in a.Rooms)
					{
						_rooms.Add(r);
					}

					foreach (var m in a.Mobiles)
					{
						_mobiles.Add(m);
					}
				}

				// Load accounts
				foreach (var account in db.Accounts.Include(a => a.Characters))
				{
					_accounts.Add(account);

					_accountsByName[account.Name.CasedName()] = account;

					foreach (var character in account.Characters)
					{
						_charactersByName[character.Name.CasedName()] = character;
					}
				}
			}

			foreach (var room in _rooms)
			{
				if (room.Name == "The Temple Of Mota")
				{
					var k = 5;
				}
			}
		}

		public static void Update(Entity entity)
		{
			using (var db = new DataContext())
			{
				db.Update(entity);
				db.SaveChanges();
			}
		}

		public static Area GetAreaById(int id) => _areas.GetById(id);

		public static Area[] GetAllAreas() => _areas.ToArray();

		public static Room GetRoomById(int id) => _rooms.GetById(id);

		public static void CreateRoom(Room r)
		{
			_rooms.Create(r);

			if (!r.Area.Rooms.Contains(r))
			{
				r.Area.Rooms.Add(r);
			}
		}

		private static void DisconnectInternal(DataContext db, Room room, Direction direction)
		{
			var oppositeDir = direction.GetOppositeDirection();

			// Delete existing connections
			var existingConnection = (from e in room.Exits
									  where e.Direction == direction
									  select e).FirstOrDefault();

			if (existingConnection != null)
			{
				db.Remove(existingConnection);

				if (existingConnection.TargetRoom != null)
				{
					var oppositeConnection = (from e in existingConnection.TargetRoom.Exits
											  where e.TargetRoomId == room.Id && e.Direction == oppositeDir
											  select e).FirstOrDefault();

					if (oppositeConnection != null)
					{
						db.Remove(oppositeConnection);
					}
				}
			}
		}

		public static void ConnectRoom(Room sourceRoom, Room targetRoom, Direction direction)
		{
			using (var db = new DataContext())
			{
				// Delete existing connections
				DisconnectInternal(db, sourceRoom, direction);

				// Create new ones
				var newConnection = new RoomExit
				{
					TargetRoomId = targetRoom.Id,
					TargetRoom = targetRoom,
					Direction = direction
				};
				sourceRoom.Exits.Add(newConnection);
				db.Update(sourceRoom);

				var oppositeNewConnection = new RoomExit
				{
					TargetRoomId = sourceRoom.Id,
					TargetRoom = sourceRoom,
					Direction = direction.GetOppositeDirection()
				};
				targetRoom.Exits.Add(oppositeNewConnection);
				db.Update(targetRoom);

				db.SaveChanges();
			}
		}

		public static void DisconnectRoom(Room room, Direction direction)
		{
			using (var db = new DataContext())
			{
				// Delete existing connections
				DisconnectInternal(db, room, direction);
				db.SaveChanges();
			}
		}

		public static Mobile GetMobileById(int id) => _mobiles.GetById(id);

		public static void CreateMobile(Mobile mobile)
		{
			_mobiles.Create(mobile);

			if (!mobile.Area.Mobiles.Contains(mobile))
			{
				mobile.Area.Mobiles.Add(mobile);
			}
		}

		public static Account GetAccountByName(string name)
		{
			name = name.CasedName();

			Account account = null;
			_accountsByName.TryGetValue(name, out account);

			return account;
		}

		public static void CreateAccount(Account account)
		{
			account.Name = account.Name.CasedName();
			_accounts.Create(account);

			_accountsByName[account.Name] = account;
		}

		public static Character GetCharacterByName(string name)
		{
			name = name.CasedName();

			Character character = null;
			_charactersByName.TryGetValue(name, out character);

			return character;
		}

		public static int CalculateCharactersAmount()
		{
			var result = 0;
			foreach (var acc in _accounts)
			{
				result += acc.Characters.Count;
			}

			return result;
		}
	}
}
