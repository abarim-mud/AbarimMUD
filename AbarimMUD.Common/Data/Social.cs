using System.Text.Json.Serialization;
using Ur;

namespace AbarimMUD.Data
{
	public class Social : IHasId<string>
	{
		public static readonly SingleFileStorageString<Social> Storage = new SingleFileStorageString<Social>("socials.json");

		[JsonIgnore]
		public string Id
		{
			get => Name;
			set => Name = value;
		}

		public string Name { get; set; }
		public string NoTargetPlayer { get; set; }
		public string NoTargetRoom { get; set; }
		public string TargettedPlayer { get; set; }
		public string TargettedTarget { get; set; }
		public string TargettedRoom { get; set; }
		public string TargetNotFoundPlayer { get; set; }
		public string SelfPlayer { get; set; }
		public string SelfRoom { get; set; }

		public void Create() => Storage.Create(this);

		public static Social GetSocialByName(string name) => Storage.GetByKey(name);
		public static Social EnsureSocialByName(string name) => Storage.EnsureByKey(name);
		public static Social LookupSocial(string name)
		{
			// Find by exact match
			var social = Storage.GetByKey(name);
			if (social != null)
			{
				return social;
			}

			// Find by starts with
			return Storage.Lookup(name);
		}

		public static void SaveSocials() => Storage.SaveAll();
	}
}
