using System;

namespace AbarimMUD.Common.Data
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

	[Flags]
	public enum MobileFlags
	{
		None = 0,
		Npc = 1 << 0,
		Sentinel = 1 << 1,
		Scavenger = 1 << 2,
		Aggressive = 1 << 5,
		StayInArea = 1 << 6,
		Wimpy = 1 << 7,
		Pet = 1 << 8,
		Train = 1 << 9,
		Practice = 1 << 10,
		Undead = 1 << 14,
		Cleric = 1 << 16,
		Mage = 1 << 17,
		Thief = 1 << 18,
		Warrior = 1 << 19,
		NoAlign = 1 << 20,
		NoPurge = 1 << 21,
		OutDoors = 1 << 22,
		InDoors = 1 << 24,
		IsHealer = 1 << 26,
		Gain = 1 << 27,
		UpdateAlways = 1 << 28,
		IsChanger = 1 << 29,
		FriendlyBits = Train | Practice | IsHealer | IsChanger
	}

	[Flags]
	public enum MobileOffensiveFlags
	{
		None = 0,
		AreaAttack = 1 << 0,
		Backstab = 1 << 1,
		Bash = 1 << 2,
		Berserk = 1 << 3,
		Disarm = 1 << 4,
		Dodge = 1 << 5,
		Fade = 1 << 6,
		Fast = 1 << 7,
		Kick = 1 << 8,
		KickDirt = 1 << 9,
		Parry = 1 << 10,
		Rescue = 1 << 11,
		Tail = 1 << 12,
		Trip = 1 << 13,
		Crush = 1 << 14,
		AssistAll = 1 << 15,
		AssistAlign = 1 << 16,
		AssistRace = 1 << 17,
		AssistPlayer = 1 << 18,
		AssistGuard = 1 << 19,
		AssistVNum = 1 << 20,
	}

	[Flags]
	public enum ResistanceFlags
	{
		None = 0,
		Summon = 1 << 0,
		Charm = 1 << 1,
		Magic = 1 << 2,
		Weapon = 1 << 3,
		Bash = 1 << 4,
		Pierce = 1 << 5,
		Slash = 1 << 6,
		Fire = 1 << 7,
		Cold = 1 << 8,
		Lightning = 1 << 9,
		Acid = 1 << 10,
		Poison = 1 << 11,
		Negative = 1 << 12,
		Holy = 1 << 13,
		Energy = 1 << 14,
		Mental = 1 << 15,
		Disease = 1 << 16,
		Drowning = 1 << 17,
		Light = 1 << 18,
		Sound = 1 << 19,
		Wood = 1 << 23,
		Silver = 1 << 24,
		Iron = 1 << 25,
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

	[Flags]
	public enum AffectedByFlags
	{
		None = 0,
		Blindness = 1 << 0,
		Invisible = 1 << 1,
		DetectEvil = 1 << 2,
		DetectInvis = 1 << 3,
		DetectMagic = 1 << 4,
		DetectHidden = 1 << 5,
		DetectGood = 1 << 6,
		Sanctuary = 1 << 7,
		FaerieFire = 1 << 8,
		Infrared = 1 << 9,
		Curse = 1 << 10,
		Poison = 1 << 12,
		ProtectEvil = 1 << 13,
		ProtectGood = 1 << 14,
		Sneak = 1 << 15,
		Hide = 1 << 16,
		Sleep = 1 << 17,
		Charm = 1 << 18,
		Flying = 1 << 19,
		PassDoor = 1 << 20,
		Haste = 1 << 21,
		Calm = 1 << 22,
		Plague = 1 << 23,
		Weaken = 1 << 24,
		DarkVision = 1 << 25,
		Berserk = 1 << 26,
		Swim = 1 << 27,
		Regeneration = 1 << 28,
		Slow = 1 << 29,
	}

	public enum AttackType
	{
		None,
		Slice,
		Stab,
		Slash,
		Whip,
		Claw,
		Blast,
		Pound,
		Crush,
		Grep,
		Bite,
		Pierce,
		Suction,
		Beating,
		Digestion,
		Charge,
		Slap,
		Punch,
		Wrath,
		Magic,
		Divine,
		Cleave,
		Scratch,
		Peck,
		PeckB,
		Chop,
		Sting,
		Smash,
		ShBite,
		FlBite,
		FrBite,
		AcBite,
		Chomp,
		Drain,
		Thrust,
		Slime,
		Shock,
		Thwack,
		Flame,
		Chill
	}

	public class Mobile: AreaEntity
	{
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string Description { get; set; }
		public Race Race { get; set; }
		public MobileFlags MobileFlags { get; set; }
		public AffectedByFlags AffectedByFlags { get; set; }
		public int Alignment { get; set; }
		public int Group { get; set; }
		public int Level { get; set; }
		public int HitRoll { get; set; }
		public string HitDice { get; set; }
		public string ManaDice { get; set; }
		public string DamageDice { get; set; }
		public AttackType AttackType { get; set; }
		public int AcPierce { get; set; }
		public int AcBash { get; set; }
		public int AcSlash { get; set; }
		public int AcExotic { get; set; }
		public MobileOffensiveFlags OffenseFlags { get; set; }
		public ResistanceFlags ImmuneFlags { get; set; }
		public ResistanceFlags ResistanceFlags { get; set; }
		public ResistanceFlags VulnerableFlags { get; set; }
		public MobilePosition StartPosition { get; set; }
		public MobilePosition DefaultPosition { get; set; }
		public Sex Sex { get; set; }
		public int Wealth { get; set; }
		public FormFlags FormsFlags { get; set; }
		public PartFlags PartsFlags { get; set; }
		public MobileSize Size { get; set; }
		public Material Material { get; set; }
	}
}
