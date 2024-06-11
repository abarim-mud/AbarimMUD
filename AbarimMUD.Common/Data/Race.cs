using AbarimMUD.Storage;

namespace AbarimMUD.Data
{
	public class Race
	{
		public static readonly MultipleFilesStorageString<Race> Storage = new MultipleFilesStorageString<Race>(r => r.Id, "races");

		public string Id { get; set; }
		public string Name { get; set; }

		public float HitpointsModifier { get; set; } = 1.0f;
		public float PenetrationModifier { get; set; } = 1.0f;
		public RaceClassValueRange NaturalArmor;
		public AttackType AttackType { get; set; }
		public RaceClassValueRange NaturalAttacksCount;
		public RaceClassValueRange BareHandedMinimumDamage;
		public RaceClassValueRange BareHandedMaximumDamage;

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Race GetRaceById(string name) => Storage.GetByKey(name);
		public static Race EnsureRaceById(string name) => Storage.EnsureByKey(name);
	} 
}
