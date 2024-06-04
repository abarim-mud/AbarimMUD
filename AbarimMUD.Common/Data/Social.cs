using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public class Social
	{
		public static readonly SingleFileStorageString<Social> Storage = new SingleFileStorageString<Social>(s => s.Name, "socials.json");

		public string Name { get; set; }
		public string CharNoArgument { get; set; }
		public string OthersNoArgument { get; set; }
		public string CharFound { get; set; }
		public string OthersFound { get; set; }
		public string VictimFound { get; set; }
		public string CharAuto { get; set; }
		public string OthersAuto { get; set; }

		public void Create() => Storage.Create(this);

		public static Social GetSocialByName(string name) => Storage.GetByKey(name);
		public static Social EnsureSocialByName(string name) => Storage.EnsureByKey(name);
		public static Social LookupSocial(string name) => Storage.Lookup(name);
		public static void SaveSocials() => Storage.SaveAll();
	}
}
