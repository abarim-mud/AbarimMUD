using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public sealed class Account : StoredInFile
	{
		public static readonly MultipleFilesStorageString<Account> Storage = new Accounts();

		public string Name { get; set; }

		public string PasswordHash { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Account GetAccountByName(string name) => Storage.GetByKey(name);
		public static Account EnsureAccountByName(string name) => Storage.EnsureByKey(name);
		public static Account LookupAccount(string name) => Storage.Lookup(name);
	}
}