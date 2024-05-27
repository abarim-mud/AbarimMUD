using AbarimMUD.Data;
using System;
using System.Collections.Generic;

namespace AbarimMUD.ImportAre
{
	[Flags]
	public enum OldRoomFlags
	{
		None = 0,
		Dark = 1 << 0,
		NoMob = 1 << 2,
		InDoors = 1 << 3,
		Private = 1 << 9,
		Safe = 1 << 10,
		Solitary = 1 << 11,
		PetShop = 1 << 12,
		NoRecall = 1 << 13,
		ImpOnly = 1 << 14,
		GodsOnly = 1 << 15,
		HeroesOnly = 1 << 16,
		NewbiesOnly = 1 << 17,
		Law = 1 << 18,
		Nowhere = 1 << 19
	}

	[Flags]
	public enum OldRoomExitFlags
	{
		None = 0,
		Door = 1 << 0,
		Closed = 1 << 1,
		Locked = 1 << 2,
		PickProof = 1 << 5,
		NoPass = 1 << 6,
		Easy = 1 << 7,
		Hard = 1 << 8,
		Infuriating = 1 << 9,
		NoClose = 1 << 10,
		NoLock = 1 << 11,
	}

	[Flags]
	public enum OldMobileFlags
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
	public enum OldMobileOffensiveFlags
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
		AssistId = 1 << 20,
	}

	[Flags]
	public enum OldResistanceFlags
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
	public enum OldAffectedByFlags
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

	public class RaceInfo
	{
		public bool PcRace { get; }
		public OldMobileFlags Flags { get; }
		public OldAffectedByFlags AffectedByFlags { get; }
		public OldMobileOffensiveFlags OffensiveFlags { get; }
		public OldResistanceFlags ImmuneFlags { get; }
		public OldResistanceFlags ResistanceFlags { get; }
		public OldResistanceFlags VulnerableFlags { get; }
		public FormFlags FormFlags { get; }
		public PartFlags PartFlags { get; }

		public RaceInfo(bool pcRace,
			OldMobileFlags oldMobileFlags, OldAffectedByFlags oldAffectedByFlags, OldMobileOffensiveFlags oldOffensiveFlags,
			OldResistanceFlags immuneFlags, OldResistanceFlags oldResistanceFlags, OldResistanceFlags vulnerableFlags,
			FormFlags formFlags, PartFlags partFlags)
		{
			PcRace = pcRace;
			Flags = oldMobileFlags;
			AffectedByFlags = oldAffectedByFlags;
			OffensiveFlags = oldOffensiveFlags;
			ImmuneFlags = immuneFlags;
			ResistanceFlags = oldResistanceFlags;
			VulnerableFlags = vulnerableFlags;
			FormFlags = formFlags;
			PartFlags = partFlags;
		}
	}

	public static class RaceExtensions
	{
		private static readonly Dictionary<Race, RaceInfo> _raceData = new Dictionary<Race, RaceInfo>
		{
			[Race.Unique] = new RaceInfo(false, 0, 0, 0, 0, 0, 0, 0, 0),
			[Race.Human] = new RaceInfo(true, 0, 0, 0, 0, 0, 0, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Elf] = new RaceInfo(true, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Charm, OldResistanceFlags.Iron,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Dwarf] = new RaceInfo(true, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Poison | OldResistanceFlags.Disease,
				OldResistanceFlags.Drowning, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Giant] = new RaceInfo(true, 0, 0, 0, 0, OldResistanceFlags.Fire | OldResistanceFlags.Cold,
				OldResistanceFlags.Mental | OldResistanceFlags.Lightning, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Pixie] = new RaceInfo(false, 0,
				OldAffectedByFlags.Flying | OldAffectedByFlags.DetectGood | OldAffectedByFlags.DetectEvil | OldAffectedByFlags.DetectMagic,
				0, 0, 0, 0, FormFlags.FormsHumanoid | FormFlags.Magical, PartFlags.PartsHumanoid | PartFlags.Wings),
			[Race.Bat] = new RaceInfo(false, 0,
				OldAffectedByFlags.Flying | OldAffectedByFlags.DarkVision, OldMobileOffensiveFlags.Dodge | OldMobileOffensiveFlags.Fast,
				0, 0, OldResistanceFlags.Light, FormFlags.Mammal, PartFlags.PartsQuadRuped | PartFlags.Wings),
			[Race.Bear] = new RaceInfo(false, 0, 0, OldMobileOffensiveFlags.Crush | OldMobileOffensiveFlags.Disarm | OldMobileOffensiveFlags.Berserk, 0,
				OldResistanceFlags.Bash | OldResistanceFlags.Cold, 0, FormFlags.Mammal, PartFlags.PartsBiped | PartFlags.Claws | PartFlags.Fangs),
			[Race.Cat] = new RaceInfo(false, 0, OldAffectedByFlags.DarkVision, OldMobileOffensiveFlags.Fast | OldMobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsFeline),
			[Race.Centipede] = new RaceInfo(false, 0, OldAffectedByFlags.DarkVision, 0, 0,
				OldResistanceFlags.Pierce | OldResistanceFlags.Cold, OldResistanceFlags.Bash,
				FormFlags.FormsBug | FormFlags.Poison, PartFlags.Head | PartFlags.Legs | PartFlags.Eye),
			[Race.Dog] = new RaceInfo(false, 0, 0, OldMobileOffensiveFlags.Fast, 0, 0, 0, FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Claws),
			[Race.Doll] = new RaceInfo(false, 0, 0, 0, OldResistanceFlags.Cold | OldResistanceFlags.Poison | OldResistanceFlags.Holy |
				OldResistanceFlags.Negative | OldResistanceFlags.Mental | OldResistanceFlags.Disease | OldResistanceFlags.Drowning,
				OldResistanceFlags.Bash | OldResistanceFlags.Light, OldResistanceFlags.Slash | OldResistanceFlags.Fire |
				OldResistanceFlags.Acid | OldResistanceFlags.Lightning | OldResistanceFlags.Energy,
				FormFlags.Other | FormFlags.Construct | FormFlags.Biped | FormFlags.ColdBlood,
				PartFlags.PartsHumanoid & ~(PartFlags.PartsAlive | PartFlags.Ear)),
			[Race.Dragon] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared | OldAffectedByFlags.Flying, 0, 0,
				OldResistanceFlags.Fire | OldResistanceFlags.Bash | OldResistanceFlags.Charm, OldResistanceFlags.Pierce | OldResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Sentinent | FormFlags.Dragon,
				PartFlags.PartsLizard | PartFlags.Fingers | PartFlags.Claws | PartFlags.Fangs),
			[Race.Fido] = new RaceInfo(false, 0, 0, OldMobileOffensiveFlags.Dodge | OldMobileOffensiveFlags.AssistRace, 0, 0,
				OldResistanceFlags.Magic, FormFlags.Mammal | FormFlags.Poison, PartFlags.PartsCanine | PartFlags.Tail),
			[Race.Fox] = new RaceInfo(false, 0, OldAffectedByFlags.DarkVision, OldMobileOffensiveFlags.Fast | OldMobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Tail),
			[Race.Goblin] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Disease, OldResistanceFlags.Magic,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Hobgoblin] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Disease | OldResistanceFlags.Poison, 0,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid | PartFlags.Tusks),
			[Race.Kobold] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Poison, OldResistanceFlags.Magic,
				FormFlags.FormsHumanoid | FormFlags.Poison, PartFlags.PartsHumanoid | PartFlags.Tail),
			[Race.Lizard] = new RaceInfo(false, 0, 0, 0, 0, OldResistanceFlags.Poison, OldResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Animal | FormFlags.Reptile | FormFlags.ColdBlood, PartFlags.PartsLizard),
			[Race.Modron] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared, OldMobileOffensiveFlags.AssistRace | OldMobileOffensiveFlags.AssistAlign,
				OldResistanceFlags.Charm | OldResistanceFlags.Disease | OldResistanceFlags.Mental | OldResistanceFlags.Holy | OldResistanceFlags.Negative,
				OldResistanceFlags.Fire | OldResistanceFlags.Cold | OldResistanceFlags.Acid, 0,
				FormFlags.Sentinent, PartFlags.PartsHumanoid & ~(PartFlags.PartsAlive | PartFlags.Fingers)),
			[Race.Orc] = new RaceInfo(false, 0, OldAffectedByFlags.Infrared, 0, 0, OldResistanceFlags.Disease, OldResistanceFlags.Light,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Pig] = new RaceInfo(false, 0, 0, 0, 0, 0, 0, FormFlags.Mammal, PartFlags.PartsQuadRuped),
			[Race.Rabbit] = new RaceInfo(false, 0, 0, OldMobileOffensiveFlags.Dodge | OldMobileOffensiveFlags.Fast, 0, 0, 0, FormFlags.Mammal,
				PartFlags.PartsQuadRuped),
			[Race.Schoolmonster] = new RaceInfo(false, OldMobileFlags.NoAlign, 0, 0, OldResistanceFlags.Charm | OldResistanceFlags.Summon, 0,
				OldResistanceFlags.Magic, FormFlags.Edible | FormFlags.Biped | FormFlags.Mammal,
				PartFlags.PartsBiped | PartFlags.Tail | PartFlags.Claws),
			[Race.Snake] = new RaceInfo(false, 0, 0, 0, 0, OldResistanceFlags.Poison, OldResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Animal | FormFlags.Reptile | FormFlags.Snake | FormFlags.ColdBlood,
				PartFlags.PartsReptile | PartFlags.Fangs),
			[Race.SongBird] = new RaceInfo(false, 0, OldAffectedByFlags.Flying, OldMobileOffensiveFlags.Dodge | OldMobileOffensiveFlags.Fast, 0, 0, 0,
				FormFlags.Bird, PartFlags.PartsBird),
			[Race.Troll] = new RaceInfo(false, 0, OldAffectedByFlags.Regeneration | OldAffectedByFlags.Infrared | OldAffectedByFlags.DetectHidden,
				OldMobileOffensiveFlags.Berserk, 0, OldResistanceFlags.Charm | OldResistanceFlags.Bash, OldResistanceFlags.Fire | OldResistanceFlags.Acid,
				FormFlags.FormsHumanoid | FormFlags.Poison, PartFlags.PartsHumanoid | PartFlags.Claws | PartFlags.Fangs),
			[Race.Waterfowl] = new RaceInfo(false, 0, OldAffectedByFlags.Swim | OldAffectedByFlags.Flying, 0, 0, OldResistanceFlags.Drowning, 0,
				FormFlags.Bird, PartFlags.PartsBird),
			[Race.Wolf] = new RaceInfo(false, 0, OldAffectedByFlags.DarkVision, OldMobileOffensiveFlags.Fast | OldMobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Claws | PartFlags.Tail),
			[Race.Wyvern] = new RaceInfo(false, 0,
				OldAffectedByFlags.Flying | OldAffectedByFlags.DetectInvis | OldAffectedByFlags.DetectHidden,
				OldMobileOffensiveFlags.Bash | OldMobileOffensiveFlags.Fast | OldMobileOffensiveFlags.Dodge,
				OldResistanceFlags.Poison, 0, OldResistanceFlags.Light,
				FormFlags.Edible | FormFlags.Poison | FormFlags.Animal | FormFlags.Dragon, PartFlags.PartsLizard | PartFlags.Fangs),
		};

		public static RaceInfo GetRaceInfo(this Race race)
		{
			RaceInfo result;
			if (!_raceData.TryGetValue(race, out result))
			{
				throw new Exception($"Could not find info for race {race}");
			}

			return result;
		}
	}
}
