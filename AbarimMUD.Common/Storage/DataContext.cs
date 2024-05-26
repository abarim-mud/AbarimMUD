using AbarimMUD.Data;
using System;

namespace AbarimMUD.Storage
{
	public class DataContext
	{
		private readonly DataContextSettings _context;

		public CRUD<Account> Accounts { get; }
		public Characters Characters { get; }
		public Areas Areas { get; }
		public Socials Socials { get; }

		public DataContext(string path, Action<string> log)
		{
			_context = new DataContextSettings(path, log);
			Accounts = new Accounts(_context);
			Characters = new Characters(_context);
			Areas = new Areas(_context);
			Socials = new Socials(_context);

			Accounts.SetReferences(this);
			Characters.SetReferences(this);
			Areas.SetReferences(this);
		}
	}
}
