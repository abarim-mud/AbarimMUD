using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum ItemType
	{
		Light,
		Scroll,
		Wand,
		Staff,
		Weapon,
		Treasure,
		Armor,
		Potion,
		Clothing,
		Furniture,
		Trash,
		Container,
		Drink,
		Key,
		Food,
		Money,
		Boat,
		NpcCorpse,
		PcCorpse,
		Fountain,
		Pill,
		Protect,
		Map,
		Portal,
		WarpStone,
		RoomKey,
		Gem,
		Jewelry,
		JukeBox
	}

	public enum WearType
	{
		None = -1,
		Light,
		FingerLeft,
		FingerRight,
		Neck1,
		Neck2,
		Body,
		Head,
		Legs,
		Feet,
		Hands,
		Arms,
		Shield,
		About,
		Waist,
		WristLeft,
		WristRight,
		Wield,
		Hold,
		Float
	}

	public enum WeaponType
	{
		Exotic,
		Sword,
		Mace,
		Dagger,
		Axe,
		Staff,
		Flail,
		Whip,
		Polearm
	}

	[Flags]
	public enum ItemExtraFlags
	{
		None = 0,
		Glow = 1 << 0,
		Humming = 1 << 1,
		Dark = 1 << 2,
		Lock = 1 << 3,
		Evil = 1 << 4,
		Invisible = 1 << 5,
		Magic = 1 << 6,
		NoDrop = 1 << 7,
		Bless = 1 << 8,
		AntiGood = 1 << 9,
		AntiEvil = 1 << 10,
		AntiNeutral = 1 << 11,
		NoRemove = 1 << 12,
		Inventory = 1 << 13,
		NoPurge = 1 << 14,
		RotDeath = 1 << 15,
		VisDeath = 1 << 16,
		NonMetal = 1 << 18,
		NoLocate = 1 << 19,
		MeltDrop = 1 << 20,
		HadTimer = 1 << 21,
		SellExtract = 1 << 22,
		BurnProof = 1 << 24,
		NounCurse = 1 << 25,
		Corroded = 1 << 26,
	}

	[Flags]
	public enum ItemWearFlags
	{
		None = 0,
		Take = 1 << 0,
		Finger = 1 << 1,
		Neck = 1 << 2,
		Body = 1 << 3,
		Head = 1 << 4,
		Legs = 1 << 5,
		Feet = 1 << 6,
		Hands = 1 << 7,
		Arms = 1 << 8,
		Shield = 1 << 9,
		About = 1 << 10,
		Waist = 1 << 11,
		Wrist = 1 << 12,
		Wield = 1 << 13,
		Hold = 1 << 14,
		NoSac = 1 << 15,
		Float = 1 << 16,
		Light = 1 << 17
	}

	public enum EffectBitType
	{
		None,
		Object,
		Immunity,
		Resistance,
		Vulnerability,
		Weapon
	}

	public enum EffectType
	{
		None = 0,
		Strength,
		Dexterity,
		Intelligence,
		Wisdom,
		Constitution,
		Sex,
		Class,
		Level,
		Age,
		Height,
		Weight,
		Mana,
		Hit,
		Move,
		Gold,
		Exp,
		Ac,
		HitRoll,
		DamRoll,
		Saves,
		SavingPara = Saves,
		SavingRod,
		SavingPetri,
		SavingBreath,
		SavingSpell,
		SpellAffect
	}

	public enum LiquidType
	{
		Water,
		Beer,
		RedWine,
		Ale,
		DarkAle,
		Whisky,
		Lemonade,
		Firebreather,
		LocalSpecialty,
		SlimeMoldJuice,
		Milk,
		Tea,
		Coffee,
		Blood,
		SaltWater,
		Coke,
		RootBeer,
		ElvishWine,
		WhiteWine,
		Champagne,
		Mead,
		RoseWine,
		BenidictineWine,
		Vodka,
		CranberryJuice,
		OrangeJuice,
		Absinthe,
		Brandy,
		Aquavit,
		Schnapps,
		Icewine,
		Amontillado,
		Sherry,
		Framboise,
		Rum,
		Cordial
	}

	public enum Skill
	{
		// Spells
		Reserved,
		AcidBlast,
		Armor,
		Bless,
		Blindness,
		BurningHands,
		CallLightning,
		Calm,
		Cancellation,
		CauseCritical,
		CauseLight,
		CauseSerious,
		ChainLightning,
		ChangeSex,
		CharmPerson,
		ChillTouch,
		ColourSpray,
		ContinualLight,
		ControlWeather,
		CreateFood,
		CreateRose,
		CreateSpring,
		CreateWater,
		CureBlindness,
		CureCritical,
		CureDisease,
		CureLight,
		CurePoison,
		CureSerious,
		Curse,
		Demonfire,
		DetectEvil,
		DetectGood,
		DetectHidden,
		DetectInvis,
		DetectMagic,
		DetectPoison,
		DispelEvil,
		DispelGood,
		DispelMagic,
		Earthquake,
		EnchantArmor,
		EnchantWeapon,
		EnergyDrain,
		FaerieFire,
		FaerieFog,
		Farsight,
		Fireball,
		Fireproof,
		Flamestrike,
		Fly,
		FloatingDisc,
		Frenzy,
		Gate,
		GiantStrength,
		Harm,
		Haste,
		Heal,
		HeatMetal,
		HolyWord,
		Identify,
		Infravision,
		Invisibility,
		KnowAlignment,
		LightningBolt,
		LocateObject,
		MagicMissile,
		MassHealing,
		MassInvis,
		Nexus,
		PassDoor,
		Plague,
		Poison,
		Portal,
		ProtectionEvil,
		ProtectionGood,
		RayOfTruth,
		Recharge,
		Refresh,
		RemoveCurse,
		Sanctuary,
		Shield,
		ShockingGrasp,
		Sleep,
		Slow,
		StoneSkin,
		Summon,
		Teleport,
		Ventriloquate,
		Weaken,
		WordOfRecall,

		// Dragon breath
		AcidBreath,
		FireBreath,
		FrostBreath,
		GasBreath,
		LightningBreath,

		// Special
		GeneralPurpose,
		HighExplosive,

		// Combat & weapon
		Axe,
		Dagger,
		Flail,
		Mace,
		Polearm,
		ShieldBlock,
		Spear,
		Sword,
		Whip,
		Backstab,
		Bash,
		Berserk,
		DirtKicking,
		Disarm,
		Dodge,
		EnhancedDamage,
		Envenom,
		HandToHand,
		Kick,
		Parry,
		Rescue,
		Trip,
		SecondAttack,
		ThirdAttack,

		// Non-combat
		FastHealing,
		Haggle,
		Hide,
		Lore,
		Meditation,
		Peek,
		PickLock,
		Sneak,
		Steal,
		Scrolls,
		Staves,
		Wands,
		Recall
	}

	public class GameObject : AreaEntity
	{
		public int? VNum { get; set; }
		public string Name { get; set; }
		public string ShortDescription { get; set; }
		public string Description { get; set; }
		public Material Material { get; set; }
		public ItemType ItemType { get; set; }
		public ItemWearFlags WearFlags { get; set; }
		public ItemExtraFlags ExtraFlags { get; set; }

		public int Value1 { get; set; }
		public int Value2 { get; set; }
		public int Value3 { get; set; }
		public int Value4 { get; set; }
		public int Value5 { get; set; }
		public int Level { get; set; }
		public int Weight { get; set; }
		public int Cost { get; set; }
		public int Condition { get; set; }
		public string ExtraKeyword { get; set; }
		public string ExtraDescription { get; set; }
	}
}