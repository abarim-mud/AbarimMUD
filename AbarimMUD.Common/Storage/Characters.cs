using AbarimMUD.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	public class Characters : MultipleFilesStorage<Character>
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

		public override string Name => "characters";

		internal Characters() : base(c => c.Name, Accounts.SubfolderName)
		{
		}

		protected override JsonSerializerOptions CreateJsonOptions()
		{
			var result = base.CreateJsonOptions();
			result.Converters.Add(Common.ClassConverter);
			result.Converters.Add(Common.ItemConverter);

			return result;
		}

		protected override void InternalLoad()
		{
			var accountsFolder = Folder;
			if (!Directory.Exists(accountsFolder))
			{
				LogDoesntExist(accountsFolder);
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
			// Add account folder
			var characterFolder = entity.BuildCharacterFolder();
			var result = Path.Combine(Folder, characterFolder);

			return Path.Combine(result, CharacterFile);
		}

		protected internal override void SetReferences()
		{
			base.SetReferences();

			foreach (var record in _tempCache)
			{
				record.Character.Account = Account.EnsureAccountByName(record.AccountName);
			}

			foreach (var character in this)
			{
				character.PlayerClass = GameClass.EnsureClassById(character.Class.Id);

				foreach (var item in character.Inventory.Items)
				{
					item.Info = Item.EnsureItemById(item.Info.Id);
				}

				foreach (var eq in character.Equipment.Items)
				{
					if (eq.Item == null)
					{
						continue;
					}

					eq.Item.Info = Item.EnsureItemById(eq.Item.Info.Id);
				}
			}

			_tempCache.Clear();
		}
	}
}
