using System.Linq;
using AbarimMUD.Data;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace AbarimMUD
{
	public abstract class BaseCRUD<T> where T : Entity
	{
		protected readonly ConcurrentDictionary<int, T> _cache = new ConcurrentDictionary<int, T>();

		protected abstract DbSet<T> GetDbSet(DataContext db);
		protected abstract IQueryable<T> UpdateQuery(IQueryable<T> query);

		public void Create(T entity)
		{
			using (var db = new DataContext())
			{
				var dbSet = GetDbSet(db);
				dbSet.Add(entity);
				db.SaveChanges();

				_cache[entity.Id] = entity;
			}
		}

		public T GetById(int id)
		{
			T result;
			if (_cache.TryGetValue(id, out result))
			{
				return result;
			}

			using (var db = new DataContext())
			{
				var dbSet = GetDbSet(db);
				var query = (from e in dbSet where e.Id == id select e);
				query = UpdateQuery(query);
				result = query.FirstOrDefault();

				if (result != null)
				{
					_cache[id] = result;
				}

				return result;
			}
		}

		public void Update(T entity)
		{
			using (var db = new DataContext())
			{
				InternalUpdate(db, entity);
				db.SaveChanges();
			}
		}

		protected virtual void InternalUpdate(DataContext db, T entity)
		{
		}
	}

	public class RoomsCRUD : BaseCRUD<Room>
	{
		protected override IQueryable<Room> UpdateQuery(IQueryable<Room> query) => query
				.Include(r => r.Area)
				.Include(r => r.Exits);

		protected override DbSet<Room> GetDbSet(DataContext db) => db.Rooms;
	}

	public class MobileInfosCRUD : BaseCRUD<Mobile>
	{
		protected override IQueryable<Mobile> UpdateQuery(IQueryable<Mobile> query) => query
				.Include(r => r.Area)
				.Include(m => m.Shop)
				.Include(m => m.SpecialAttacks);

		protected override DbSet<Mobile> GetDbSet(DataContext db) => db.Mobiles;
	}

	public class CharactersCRUD : BaseCRUD<Character>
	{
		protected override IQueryable<Character> UpdateQuery(IQueryable<Character> query) => query
				.Include(c => c.Account);

		protected override DbSet<Character> GetDbSet(DataContext db) => db.Characters;

		public Character GetByName(string name)
		{
			var result = (from pair in _cache where pair.Value.Name == name select pair.Value).FirstOrDefault();
			if (result != null)
			{
				return result;
			}

			using (var db = new DataContext())
			{
				var query = (from e in db.Characters where e.Name == name select e);
				query = UpdateQuery(query);
				result = query.FirstOrDefault();

				if (result != null)
				{
					_cache[result.Id] = result;
				}

				return result;
			}
		}
	}

	public class AccountsCRUD : BaseCRUD<Account>
	{
		protected override IQueryable<Account> UpdateQuery(IQueryable<Account> query) => query
				.Include(a => a.Characters);

		protected override DbSet<Account> GetDbSet(DataContext db) => db.Accounts;
		public Account GetByName(string name)
		{
			var result = (from pair in _cache where pair.Value.Name == name select pair.Value).FirstOrDefault();
			if (result != null)
			{
				return result;
			}

			using (var db = new DataContext())
			{
				var query = (from e in db.Accounts where e.Name == name select e);
				query = UpdateQuery(query);
				result = query.FirstOrDefault();

				if (result != null)
				{
					_cache[result.Id] = result;
				}

				return result;
			}
		}
	}

	public static class Database
	{
		public static AccountsCRUD Accounts { get; } = new AccountsCRUD();
		public static CharactersCRUD Characters { get; } = new CharactersCRUD();
		public static RoomsCRUD Rooms { get; } = new RoomsCRUD();
		public static MobileInfosCRUD MobileInfos { get; } = new MobileInfosCRUD();
	}
}
