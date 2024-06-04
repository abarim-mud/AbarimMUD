using AbarimMUD.Storage;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum Role
	{
		Player,
		Builder,
		Administrator,
		Owner
	}

	public sealed class Character : StoredInFile
	{
		public static readonly MultipleFilesStorageString<Character> Storage = new Characters();

		[JsonIgnore]
		public Account Account { get; set; }

		public string Name { get; set; }

		public string GameClassName { get; set; }

		public Role Role { get; set; }

		[JsonIgnore]
		public bool IsStaff
		{
			get { return Role >= Role.Builder; }
		}

		public bool IsMale { get; set; }

		public int CurrentRoomId { get; set; }

		public int CurrentHP { get; set; }
		public int CurrentIP { get; set; }
		public int CurrentMV { get; set; }

		[JsonIgnore]
		public object Tag { get; set; }

		public Character()
		{
			Role = Role.Player;
			IsMale = true;

			CurrentHP = 200;
			CurrentIP = 100;
			CurrentMV = 250;
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public string BuildCharacterFolder()
		{
			var result = Account.BuildAccountFolder();

			// Add character name in the path
			result = Path.Combine(result, Name);

			return result;
		}

		public static Character GetCharacterByName(string name) => Storage.GetByKey(name);
		public static Character EnsureCharacterByName(string name) => Storage.EnsureByKey(name);
		public static Character LookupCharacter(string name) => Storage.Lookup(name);
		public static Character[] GetCharactersByAccountName(string name)
		{
			var result = (from c in Storage where c.Account.Name.ToLower() == name.ToLower() select c).ToArray();

			return result;
		}
	}
}