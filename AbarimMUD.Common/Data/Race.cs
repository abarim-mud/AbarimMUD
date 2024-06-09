using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public class Race
	{
		public static readonly MultipleFilesStorageString<Race> Storage = new MultipleFilesStorageString<Race>(r => r.Name, "races");

		public string Name { get; set; }

		public override string ToString() => Name;

		public static Race GetRaceByName(string name) => Storage.GetByKey(name);
		public static Race EnsureRaceByName(string name) => Storage.EnsureByKey(name);
		public static Race LookupRace(string name) => Storage.Lookup(name);
	} 
}
