using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum MobilePosition
	{
		Dead,
		Mort,
		Incap,
		Stun,
		Sleep,
		Rest,
		Sit,
		Fight,
		Stand
	}

	public enum Sex
	{
		None,
		Neutral,
		Male,
		Female,
		Either
	}

	public enum MobileSize
	{
		Tiny,
		Small,
		Medium,
		Large,
		Huge,
		Giant
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
		AssistRace,
		AssistAlign,
		AssistPlayer,
		AssistAll,
		AssistGuard,
		AssistVNum,

		AssistPlayers = AssistPlayer,
	}

	public enum AffectedByFlags
	{
		Infrared,
		Flying,
		DetectIllusion,
		DetectEvil,
		DetectGood,
		DetectHidden,
		DetectInvis,
		DetectMagic,
		Haste,
		Sanctuary,
		Hide,
		PassDoor,
		DarkVision,
		AcuteVision,
		Sneak,
		ProtectionEvil,
		ProtectionGood,
		Plague,
		Berserk,
		Invisible,
		Swim,
		Slow,
		FaerieFire,
		Regeneration,
		Weaken,
		Blind,
		Poison,
		Curse,
		Camouflage,

		ProtectEvil = ProtectionEvil,
	}

	public enum ResistanceFlags
	{
		Disease,
		Poison,
		Fire,
	}

	[Flags]
	public enum FormFlags
	{
		None = 0,
		// Body Forms
		Edible = 1 << 0,
		Poison = 1 << 1,
		Magical = 1 << 2,
		InstantDecay = 1 << 3,
		Other = 1 << 4,

		// Actual Forms
		Animal = 1 << 6,
		Sentinent = 1 << 7,
		Undead = 1 << 8,
		Construct = 1 << 9,
		Mist = 1 << 10,
		Intangible = 1 << 11,
		Biped = 1 << 12,
		Centaur = 1 << 13,
		Insect = 1 << 14,
		Spider = 1 << 15,
		Crustacean = 1 << 16,
		Worm = 1 << 17,
		Blob = 1 << 18,
		Mammal = 1 << 21,
		Bird = 1 << 22,
		Reptile = 1 << 23,
		Snake = 1 << 24,
		Dragon = 1 << 25,
		Amphibian = 1 << 26,
		Fish = 1 << 27,
		ColdBlood = 1 << 28,

		FormsHumanoid = Edible | Sentinent | Biped | Mammal,
		FormsMammal = Edible | Animal | Mammal,
		FormsBird = Edible | Animal | Bird,
		FormsBug = Edible | Animal | Insect
	}

	[Flags]
	public enum PartFlags
	{
		None = 0,
		Head = 1 << 0,
		Arms = 1 << 1,
		Legs = 1 << 2,
		Heart = 1 << 3,
		Brains = 1 << 4,
		Guts = 1 << 5,
		Hands = 1 << 6,
		Feet = 1 << 7,
		Fingers = 1 << 8,
		Ear = 1 << 9,
		Eye = 1 << 10,
		LongTongue = 1 << 11,
		EyeStalks = 1 << 12,
		Tentacles = 1 << 13,
		Fins = 1 << 14,
		Wings = 1 << 15,
		Tail = 1 << 16,
		Claws = 1 << 20,
		Fangs = 1 << 21,
		Horns = 1 << 22,
		Scales = 1 << 23,
		Tusks = 1 << 24,

		PartsAlive = Heart | Brains | Guts,
		PartsQuadRuped = Head | Legs | PartsAlive | Feet | Ear | Eye,
		PartsBiped = Head | Arms | Legs | PartsAlive | Feet | Ear | Eye,
		PartsHumanoid = PartsBiped | Hands | Fingers,
		PartsFeline = PartsQuadRuped | Fangs | Tail | Claws,
		PartsCanine = PartsQuadRuped | Fangs,
		PartsReptile = PartsAlive | Head | Eye | LongTongue | Tail | Scales,
		PartsLizard = PartsQuadRuped | PartsReptile,
		PartsBird = PartsAlive | Head | Legs | Feet | Eye | Wings
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

	public enum Alignment
	{
		Good,
		Neutral,
		Evil
	}

	public class Dice
	{
		public int Sides { get; set; }
		public int Count { get; set; }
		public int Bonus { get; set; }

		public Dice()
		{
		}

		public Dice(int sides, int count, int bonus)
		{
			Sides = sides;
			Count = count;
			Bonus = bonus;
		}

		public override string ToString() => $"{Count}d{Sides}+{Bonus}";
	}

	public class Mobile : AreaEntity
	{
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public int ArmorClass { get; set; }
		public List<Attack> Attacks { get; set; } = new List<Attack>();
		public Race Race { get; set; }
		public HashSet<MobileFlags> Flags { get; set; }
		public HashSet<AffectedByFlags> AffectedByFlags { get; set; }
		public Alignment Alignment { get; set; }
		public int Group { get; set; }
		public int Level { get; set; }
		public int HitRoll { get; set; }
		public Dice HitDice { get; set; }
		public Dice ManaDice { get; set; }
		public Dice DamageDice { get; set; }
		public AttackType AttackType { get; set; }
		public int AcPierce { get; set; }
		public int AcBash { get; set; }
		public int AcSlash { get; set; }
		public int AcExotic { get; set; }
		public HashSet<ResistanceFlags> ImmuneFlags { get; set; }
		public HashSet<ResistanceFlags> ResistanceFlags { get; set; }
		public HashSet<ResistanceFlags> VulnerableFlags { get; set; }
		public MobilePosition StartPosition { get; set; }
		public MobilePosition DefaultPosition { get; set; }
		public Sex Sex { get; set; }
		public int Wealth { get; set; }
		public FormFlags FormsFlags { get; set; }
		public PartFlags PartsFlags { get; set; }
		public MobileSize Size { get; set; }
		public Material Material { get; set; }

		public Shop Shop { get; set; }
		public List<MobileSpecialAttack> SpecialAttacks { get; } = new List<MobileSpecialAttack>();

		public override string ToString() => $"{Name} (#{Id})";
	}
}