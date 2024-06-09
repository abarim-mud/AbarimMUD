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
				LogDoesntExist(Folder);
				return new string[0];
			}

			return Directory.EnumerateFiles(Folder, AccountFileName, SearchOption.AllDirectories).ToArray();
		}

		protected override string BuildPath(Account entity)
		{
			// Add account folder
			var accountFolder = entity.BuildAccountFolder();
			var result = Path.Combine(Folder, accountFolder);

			return Path.Combine(result, AccountFileName);
		}
	}
}
