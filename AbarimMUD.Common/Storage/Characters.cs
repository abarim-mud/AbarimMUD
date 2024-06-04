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


		private const string CharactersSubfolder = "characters";

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

					var charactersFolder = Path.Combine(subfolder2, CharactersSubfolder);
					if (!Directory.Exists(charactersFolder))
					{
						Log($"WARNING: Subfolder {subfolder} exists, but there's no {CharactersSubfolder}");
						continue;
					}

					Log($"Loading {charactersFolder}");

					var characterFiles = Directory.GetFiles(charactersFolder);
					foreach (var characterFile in characterFiles)
					{
						var character = LoadEntity(characterFile);
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

			// Add character subfolder
			result = Path.Combine(result, CharactersSubfolder);

			// Add character name
			result = Path.Combine(result, entity.Name);
			result = Path.ChangeExtension(result, "json");

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
