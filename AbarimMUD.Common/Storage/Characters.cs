using AbarimMUD.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AbarimMUD.Storage
{
	internal class Characters : MultipleFilesStorage<Character>
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
			result.Converters.Add(Common.PlayerClassConverter);
			result.Converters.Add(Common.ItemConverter);
			result.Converters.Add(Common.SkillConverter);
			result.Converters.Add(Common.SkillValueConverter);

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

			// Reference accounts
			foreach (var record in _tempCache)
			{
				record.Character.Account = Account.EnsureAccountByName(record.AccountName);
			}

			_tempCache.Clear();

			// Reference classes, items, skills, etc
			foreach (var ch in this)
			{
				ch.Class = PlayerClass.EnsureClassById(ch.Class.Id);

				foreach(var wearItem in ch.Equipment.Items)
				{
					wearItem.Item.Info = Item.EnsureItemById(wearItem.Item.Info.Id);
				}

				foreach(var invItem in ch.Inventory.Items)
				{
					invItem.Item.Info = Item.EnsureItemById(invItem.Item.Info.Id);
				}

				if (ch.Skills != null)
				{
					foreach (var pair in ch.Skills)
					{
						pair.Value.Skill = Skill.EnsureSkillById(pair.Key);
					}
				}
			}
		}
	}
}
