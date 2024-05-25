using AbarimMUD.Data;

namespace AbarimMUD.Storage
{
	public abstract class CRUD<T> : BaseCRUD<T> where T : Entity
	{
		internal CRUD(DataContextSettings context) : base(context)
		{
		}

		public void Update(T entity)
		{
			// Save the data
			Save(entity);

			// Add to the cache
			AddToCache(entity);
		}

		internal abstract void Save(T entity);

		internal virtual void SetReferences(DataContext db)
		{
		}
	}
}
