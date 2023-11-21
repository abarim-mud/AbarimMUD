using System.Linq;
using System.Collections.Concurrent;
using AbarimMUD.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public static class Database
	{
		public interface IBaseCRUD<T> where T : Entity
		{
			T[] GetAll();
			T GetById(int id);
			void Create(T entity);
			void Update(T entity);
		}

		public interface IRoomsCRUD : IBaseCRUD<Room>
		{
			void Connect(Room sourceRoom, Room targetRoom, Direction direction);
			void Disconnect(Room sourceRoom, Direction direction);
		}

		public interface IAccountsCRUD : IBaseCRUD<Account>
		{
			Account GetByName(string name);
		}

		public interface ICharactersCRUD : IBaseCRUD<Character>
		{
			Character GetByName(string name);
			Character[] GetByAccount(int accountId);
		}

		private interface ICacheInfo<ValueT> where ValueT : Entity
		{
			void Add(ValueT entity);
		}

		private class CacheInfo<KeyT, ValueT> : ICacheInfo<ValueT> where ValueT : Entity
		{
			private readonly Func<ValueT, KeyT> _keyGetter;
			public ConcurrentDictionary<KeyT, ValueT> Cache { get; } = new ConcurrentDictionary<KeyT, ValueT>();

			public Func<ValueT, KeyT> KeyGetter => _keyGetter;

			public CacheInfo(Func<ValueT, KeyT> keyGetter)
			{
				_keyGetter = keyGetter ?? throw new ArgumentNullException(nameof(keyGetter));
			}

			public void Add(ValueT entity)
			{
				var id = _keyGetter(entity);
				Cache[id] = entity;
			}

			public bool TryGetValue(KeyT key, out ValueT result) => Cache.TryGetValue(key, out result);
		}

		private abstract class BaseCRUD<T> : IBaseCRUD<T> where T : Entity
		{
			private bool _entireDataLoaded = false;
			private readonly ConcurrentBag<ICacheInfo<T>> _caches = new ConcurrentBag<ICacheInfo<T>>();
			private readonly CacheInfo<int, T> _cacheById;

			public BaseCRUD()
			{
				// By default there should cache by id
				_cacheById = AddCache(t => t.Id);
			}

			public CacheInfo<KeyT, T> AddCache<KeyT>(Func<T, KeyT> idGetter)
			{
				var result = new CacheInfo<KeyT, T>(idGetter);
				_caches.Add(result);

				return result;
			}

			public void AddToCaches(T entity)
			{
				foreach (var cache in _caches)
				{
					cache.Add(entity);
				}
			}

			public virtual void LoadData(DataContext db)
			{
				var entries = GetDataSet(db).ToArray();
				foreach (var e in entries)
				{
					AddToCaches(e);
					OnLoaded(db, e);
				}

				_entireDataLoaded = true;
			}

			protected T Get<KeyT>(CacheInfo<KeyT, T> cache, KeyT key, Func<T, bool> predicate)
			{
				T result = null;
				if (cache.TryGetValue(key, out result) || _entireDataLoaded)
				{
					// No need to check db if entire data had been loaded
					return result;
				}

				IQueryable<T> query;

				using (var db = new DataContext())
				{
					result = GetDataSet(db).Where(predicate).Select(e => e).FirstOrDefault();
					if (result != null)
					{
						AddToCaches(result);
						OnLoaded(db, result);
					}
				}

				return result;
			}

			public T GetById(int id) => Get(_cacheById, id, e => e.Id == id);

			public void Create(T entity)
			{
				using (var db = new DataContext())
				{
					GetDataSet(db).Add(entity);
					db.SaveChanges();

					AddToCaches(entity);
					OnCreated(db, entity);
				}
			}

			public void Update(T entity)
			{
				using (var db = new DataContext())
				{
					db.Update(entity);
					db.SaveChanges();
				}
			}

			public T[] GetAll()
			{
				if (!_entireDataLoaded)
				{
					using (var db = new DataContext())
					{
						LoadData(db);
					}
				}

				return _cacheById.Cache.Values.ToArray();
			}


			protected abstract DbSet<T> GetDataSet(DataContext db);

			protected virtual void OnLoaded(DataContext db, T entity)
			{
			}

			protected virtual void OnCreated(DataContext db, T entity)
			{
			}
		}

		private class AreasCRUD : BaseCRUD<Area>
		{
			protected override DbSet<Area> GetDataSet(DataContext db) => db.Areas;
		}

		private class RoomsCRUD : BaseCRUD<Room>, IRoomsCRUD
		{
			public override void LoadData(DataContext db)
			{
				base.LoadData(db);

				var roomsExitsDict = new Dictionary<int, List<RoomExit>>();
				var roomsExits = db.RoomsExits.ToArray();
				foreach (var exit in roomsExits)
				{
					exit.SourceRoom = GetById(exit.SourceRoomId);
					if (exit.TargetRoomId != null)
					{
						exit.TargetRoom = GetById(exit.TargetRoomId.Value);
					}

					List<RoomExit> currentRoomExits;
					if (!roomsExitsDict.TryGetValue(exit.SourceRoomId, out currentRoomExits))
					{
						currentRoomExits = new List<RoomExit>();
						roomsExitsDict[exit.SourceRoomId] = currentRoomExits;
					}

					currentRoomExits.Add(exit);
				}

				foreach (var pair in roomsExitsDict)
				{
					var room = GetById(pair.Key);

					if (room.Name == "The Temple Of Mota")
					{ 
						var k = 5;
					}

					room.Exits = pair.Value.ToArray();
				}
			}

			private static void DisconnectInternal(DataContext db, Room room, Direction direction)
			{
				var oppositeDir = direction.GetOppositeDirection();
				// Delete existing connections
				var existingConnection = (from e in db.RoomsExits
										  where e.SourceRoomId == room.Id &&
										  e.Direction == direction
										  select e).FirstOrDefault();

				if (existingConnection != null)
				{
					db.Remove(existingConnection);

					if (existingConnection.TargetRoomId != null)
					{
						var oppositeConnection = (from e in db.RoomsExits
												  where e.SourceRoomId == existingConnection.TargetRoomId.Value &&
												  e.TargetRoomId == room.Id &&
												  e.Direction == oppositeDir
												  select e).FirstOrDefault();

						if (oppositeConnection != null)
						{
							db.Remove(existingConnection);
						}
					}
				}
			}

			private static void UpdateRoomExits(DataContext db, Room room)
			{
				room.Exits = (from e in db.RoomsExits where e.SourceRoomId == room.Id select e).ToArray();
			}

			public void Connect(Room sourceRoom, Room targetRoom, Direction direction)
			{
				using (var db = new DataContext())
				{
					// Delete existing connections
					DisconnectInternal(db, sourceRoom, direction);

					// Create new ones
					var newConnection = new RoomExit
					{
						SourceRoomId = sourceRoom.Id,
						SourceRoom = sourceRoom,
						TargetRoomId = targetRoom.Id,
						TargetRoom = targetRoom,
						Direction = direction
					};
					db.RoomsExits.Add(newConnection);

					var oppositeNewConnection = new RoomExit
					{
						SourceRoomId = targetRoom.Id,
						SourceRoom = targetRoom,
						TargetRoomId = sourceRoom.Id,
						TargetRoom = sourceRoom,
						Direction = direction.GetOppositeDirection()
					};
					db.RoomsExits.Add(oppositeNewConnection);
					db.SaveChanges();

					UpdateRoomExits(db, sourceRoom);
					UpdateRoomExits(db, targetRoom);
				}
			}

			public void Disconnect(Room room, Direction direction)
			{
				using (var db = new DataContext())
				{
					// Delete existing connections
					DisconnectInternal(db, room, direction);
					db.SaveChanges();

					UpdateRoomExits(db, room);
				}
			}
			protected override DbSet<Room> GetDataSet(DataContext db) => db.Rooms;
		}

		private class MobilesCRUD : BaseCRUD<Mobile>
		{
			protected override DbSet<Mobile> GetDataSet(DataContext db) => db.Mobiles;
		}

		private class AccountsCRUD : BaseCRUD<Account>, IAccountsCRUD
		{
			private readonly CacheInfo<string, Account> _cacheByName;

			protected override DbSet<Account> GetDataSet(DataContext db) => db.Accounts;

			public AccountsCRUD()
			{
				_cacheByName = AddCache(e => e.Name);
			}

			public Account GetByName(string name)
			{
				name = name.CasedName();

				return Get(_cacheByName, name, e => e.Name == name);
			}
		}

		private class CharactersCRUD : BaseCRUD<Character>, ICharactersCRUD
		{
			private readonly CacheInfo<string, Character> _cacheByName;
			private readonly ConcurrentDictionary<int, Character[]> _charactersByAccount = new ConcurrentDictionary<int, Character[]>();

			public CharactersCRUD()
			{
				_cacheByName = AddCache(e => e.Name);
			}

			protected override DbSet<Character> GetDataSet(DataContext db) => db.Characters;

			public Character GetByName(string name)
			{
				name = name.CasedName();

				return Get(_cacheByName, name, e => e.Name == name);
			}

			public Character[] GetByAccount(int accountId)
			{
				Character[] result;
				if (_charactersByAccount.TryGetValue(accountId, out result))
				{
					return result;
				}

				using (var db = new DataContext())
				{
					result = (from c in db.Characters where c.AccountId == accountId select c).ToArray();
					foreach(var r in result)
					{
						r.Account = Accounts.GetById(accountId);
					}

					_charactersByAccount[accountId] = result;
				}

				return result;
			}

			protected override void OnCreated(DataContext db, Character entity)
			{
				base.OnCreated(db, entity);

				entity.Account = Accounts.GetById(entity.AccountId);

				Character[] characters;
				_charactersByAccount.TryRemove(entity.Account.Id, out characters);
			}
		}

		private static AreasCRUD _areasCRUD = new AreasCRUD();
		private static RoomsCRUD _roomsCrud = new RoomsCRUD();
		private static MobilesCRUD _mobilesInfosCrud = new MobilesCRUD();

		public static IBaseCRUD<Area> Areas => _areasCRUD;
		public static IRoomsCRUD Rooms => _roomsCrud;
		public static IBaseCRUD<Mobile> MobilesInfos => _mobilesInfosCrud;
		public static IAccountsCRUD Accounts { get; } = new AccountsCRUD();
		public static ICharactersCRUD Characters { get; } = new CharactersCRUD();

		public static void Initialize()
		{
			using (var db = new DataContext())
			{
				_areasCRUD.LoadData(db);
				_roomsCrud.LoadData(db);
				_mobilesInfosCrud.LoadData(db);
			}
		}
	}
}
