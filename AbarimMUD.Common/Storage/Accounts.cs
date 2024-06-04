using AbarimMUD.Data;
using System.IO;
using System.Linq;

namespace AbarimMUD.Storage
{
	internal class Accounts : MultipleFilesStorageString<Account>
	{
		internal const string SubfolderName = "accounts";
		private const string AccountFileName = "account.json";

		public Accounts() : base(a => a.Name, SubfolderName)
		{
		}

		protected override string[] ListFiles()
		{
			if (!Directory.Exists(Folder))
			{
				Log($"WARNING: Folder '{Folder}' doesnt exist.");
				return new string[0];
			}

			return Directory.EnumerateFiles(Folder, AccountFileName, SearchOption.AllDirectories).ToArray();
		}

		protected override string BuildPath(Account entity)
		{
			var result = Folder;

			// Add first letter of the account name in the path
			result = Path.Combine(result, entity.Name[0].ToString());

			// Add account name in the path
			result = Path.Combine(result, entity.Name);

			return Path.Combine(result, AccountFileName);
		}
	}
}
