using AbarimMUD.Data;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Accounts : CRUD<Account>
	{
		private const string AccountFileName = "account.json";
		internal const string SubfolderName = "accounts";

		internal string Folder => Path.Combine(BaseFolder, SubfolderName);


		public Accounts(DataContext context) : base(context)
		{
			Load();
		}

		private void Load()
		{
			if (!Directory.Exists(Folder))
			{
				return;
			}

			var subfolders = Directory.GetDirectories(Folder);
			foreach (var subfolder in subfolders)
			{
				var accountPath = Path.Combine(subfolder, AccountFileName);
				if (!File.Exists(accountPath))
				{
					Log($"WARNING: Subfolder {subfolder} exists, but there's no {AccountFileName}");
					continue;
				}

				Log($"Loading {accountPath}");

				var data = File.ReadAllText(accountPath);
				var account = JsonSerializer.Deserialize<Account>(data);
				AddToCache(account);
			}
		}

		internal override void Save(Account entity)
		{
			var accountFolder = Path.Combine(Folder, entity.Name);
			if (!Directory.Exists(accountFolder))
			{
				Directory.CreateDirectory(accountFolder);
			}

			var options = new JsonSerializerOptions { WriteIndented = true };
			var data = JsonSerializer.Serialize(entity, options);

			var accountPath = Path.Combine(accountFolder, AccountFileName);
			File.WriteAllText(accountPath, data);
		}
	}
}
