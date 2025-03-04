﻿using AbarimMUD.Storage;
using System;
using System.IO;

namespace AbarimMUD.Data
{
	public sealed class Account
	{
		public static readonly MultipleFilesStorage<Account> Storage = new Accounts();

		public string Name { get; set; }

		public string PasswordHash { get; set; }

		public DateTime LastLogin { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public string BuildAccountFolder()
		{
			// Add first letter of the account name in the path
			var result = Name[0].ToString();

			// Add account name in the path
			result = Path.Combine(result, Name);

			return result;
		}

		public static Account GetAccountByName(string name) => Storage.GetByKey(name);
		public static Account EnsureAccountByName(string name) => Storage.EnsureByKey(name);
		public static Account LookupAccount(string name) => Storage.Lookup(name);
	}
}