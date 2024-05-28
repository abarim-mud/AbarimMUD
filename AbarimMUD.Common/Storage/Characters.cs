using AbarimMUD.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public class Characters : CRUD<Character>
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


		private readonly List<CharacterRecord> _tempCache = new List<CharacterRecord>();

		private const string CharacterFileName = "character.json";

		private string AccountsFolder => Path.Combine(BaseFolder, Accounts.SubfolderName);

		internal Characters(DataContextSettings context) : base(context)
		{
			Load();
		}

		private void Load()
		{
			var accountsFolder = AccountsFolder;
			if (!Directory.Exists(accountsFolder))
			{
				return;
			}

			var subfolders = Directory.GetDirectories(accountsFolder);
			foreach (var subfolder in subfolders)
			{
				var accountName = Path.GetFileName(subfolder);
				Log($"Loading characters of account {accountName}, folder `{subfolder}`");

				var subfolders2 = Directory.GetDirectories(subfolder);
				foreach (var subfolder2 in subfolders2)
				{
					var characterPath = Path.Combine(subfolder2, CharacterFileName);
					if (!File.Exists(characterPath))
					{
						Log($"WARNING: Subfolder {subfolder} exists, but there's no {CharacterFileName}");
						continue;
					}

					Log($"Loading {characterPath}");

					var data = File.ReadAllText(characterPath);
					var options = Utility.CreateDefaultOptions();
					var character = JsonSerializer.Deserialize<Character>(data, options);

					var record = new CharacterRecord(character, accountName);
					_tempCache.Add(record);
				}
			}
		}

		internal override void SetReferences(DataContext db)
		{
			base.SetReferences(db);

			foreach(var record in _tempCache)
			{
				record.Character.Account = db.Accounts.EnsureById(record.AccountName);
				AddToCache(record.Character);
			}

			_tempCache.Clear();
		}

		public Character[] GetByAccountName(string accountName)
		{
			return (from pair in _cache where pair.Value.Account.Name == accountName select pair.Value).ToArray();
		}

		internal override void Save(Character entity)
		{
			if (entity.Account == null || string.IsNullOrEmpty(entity.Account.Name))
			{
				throw new Exception($"Character {entity.Name} account isn't set.");
			}

			var accountFolder = Path.Combine(Path.Combine(BaseFolder, Accounts.SubfolderName), entity.Account.Name);
			if (!Directory.Exists(accountFolder))
			{
				throw new Exception($"Account folder '{accountFolder}' doesnt exist");
			}

			var characterFolder = Path.Combine(accountFolder, entity.Name);
			if (!Directory.Exists(characterFolder))
			{
				Directory.CreateDirectory(characterFolder);
			}

			var options = Utility.CreateDefaultOptions();
			var data = JsonSerializer.Serialize(entity, options);

			var charPath = Path.Combine(characterFolder, CharacterFileName);
			File.WriteAllText(charPath, data);
		}
	}
}
