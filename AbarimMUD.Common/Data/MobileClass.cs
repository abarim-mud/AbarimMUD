using AbarimMUD.Attributes;
using AbarimMUD.Utils;
using Ur;

namespace AbarimMUD.Data
{
	public class MobileClass : IStoredInFile
	{
		public static readonly MultipleFilesStorage<MobileClass> Storage = new MultipleFilesStorage<MobileClass>("mobileClasses");

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }

		public LeveledValue? HolyResistance { get; set; }
		public LeveledValue? FireResistance { get; set; }
		public LeveledValue? ColdResistance { get; set; }
		public LeveledValue? ShockResistance { get; set; }
		public bool IsEthereal { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static MobileClass GetClassById(string name) => Storage.GetByKey(name);
		public static MobileClass EnsureClassById(string name) => Storage.EnsureByKey(name);

	}
}
