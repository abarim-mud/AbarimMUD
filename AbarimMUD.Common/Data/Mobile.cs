using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum Sex
	{
		None,
		Neutral,
		Male,
		Female,
		Either
	}

	public enum MobileFlags
	{
		Sentinel,
		StayArea,
		NoWander,
		NPC,
		Bash,
		Berserk,
		Disarm,
		Dodge,
		Kick,
		DirtKick,
		Parry,
		Trip,
		AreaAttack,
		Aggressive,
		Undead,
		Wimpy,
		Warrior,
		Mage,
		Cleric,
		Thief,
		Healer,
		Scavenger,
		ShopKeeper,
		Gain,
		Banker,
		NoPurge,
		GuildMaster,
		NoAlign,
		Pet,
		Fade,
		Changer,
		Rescue,
		Outdoors,
		Indoors,
		NoTrack,
		Practice,
		UpdateAlways,
		Backstab,
		Crush,
		Fast,
		Tail,
		AssistRace,
		AssistAlign,
		AssistPlayer,
		AssistAll,
		AssistGuard,
		AssistId,

		AssistPlayers = AssistPlayer,
		Train = GuildMaster,
		IsHealer = Healer,
		IsChanger = Changer,
		AssistVNum = AssistId,
	}

	public enum AttackType
	{
		Hit,
		Slice,
		Stab,
		Slash,
		Whip,
		Claw,
		Hack,
		Blast,
		Pound,
		Crush,
		Grep,
		Bite,
		Pierce,
		Suction,
		Beating,
		Charge,
		Slap,
		Punch,
		Cleave,
		Scratch,
		Peck,
		Chop,
		Sting,
		Smash,
		Chomp,
		Thrust,
		Slime,
		Shock
	}

	public class Mobile
	{
		public static readonly MultipleFilesStorageString<Mobile> Storage = new Mobiles();

		public string Id { get; set; }
		public string Name { get; set; }
		public Race Race { get; set; }
		public GameClass Class { get; set; }
		public int Level { get; set; }
		public Sex Sex { get; set; }
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();
		public int Wealth { get; set; }

		public Shop Shop { get; set; }
		public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Mobile GetMobileById(string id) => Storage.GetByKey(id);
		public static Mobile EnsureMobileById(string id) => Storage.EnsureByKey(id);

	}
}