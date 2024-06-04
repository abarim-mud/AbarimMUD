using AbarimMUD.Data;
using System.Collections.Generic;
using System.IO;

namespace AbarimMUD.Storage
{
	public class Characters : MultipleFilesStorageString<Character>
	{
		private class CharacterRecord
		{
			public Character Character { get; }
			public string AccountName { get; }

			public CharacterRecord(Character character, string accountName)
			{
				Character = character;
				AccountName = accountName;
			}
		}


		private const string CharacterFile = "character.json";

		private readonly List<CharacterRecord> _tempCache = new List<CharacterRecord>();

		internal Characters() : base(c => c.Name, Accounts.SubfolderName)
		{
		}

		protected override void InternalLoad()
		{
			var accountsFolder = Folder;
			if (!Directory.Exists(accountsFolder))
			{
				return;
			}

			var subfolders = Directory.GetDirectories(accountsFolder);
			foreach (var subfolder in subfolders)
			{
				var subfolders2 = Directory.GetDirectories(subfolder);
				foreach (var subfolder2 in subfolders2)
				{
					var accountName = Path.GetFileName(subfolder2);
					Log($"Loading characters of account {accountName}, folder `{subfolder2}`");

					var subFolders3 = Directory.GetDirectories(subfolder2);
					foreach (var subFolder3 in subFolders3)
					{
						var path = Path.Combine(subFolder3, CharacterFile);
						if (!File.Exists(path))
						{
							Log($"WARNING: Subfolder {subFolder3} exists, but there's no {CharacterFile}");
							continue;
						}

						var character = LoadEntity(path);
						AddToCache(character);
						var record = new CharacterRecord(character, accountName);
						_tempCache.Add(record);
					}
				}
			}
		}

		protected override string BuildPath(Character entity)
		{
			var result = Folder;

			// Add first letter of the account name in the path
			result = Path.Combine(result, entity.Account.Name[0].ToString());

			// Add account name in the path
			result = Path.Combine(result, entity.Account.Name);

			// Add character name in the path
			result = Path.Combine(result, entity.Name);

			// Add character file name
			result = Path.Combine(result, CharacterFile);

			return result;
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var record in _tempCache)
			{
				record.Character.Account = Account.EnsureAccountByName(record.AccountName);
			}

			_tempCache.Clear();
		}
	}
}
