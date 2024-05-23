using AbarimMUD.Data;
using System;

namespace AbarimMUD.Storage
{
	public class Database
	{
		private readonly DataContext _context;

		public CRUD<Account> Accounts { get; }
		public Characters Characters { get; }
		public CRUD<Area> Areas { get; }


		public Database(string path, Action<string> log)
		{
			_context = new DataContext(path, log);
			Accounts = new Accounts(_context);
			Characters = new Characters(_context);
			Areas = new Areas(_context);
		}
	}
}
