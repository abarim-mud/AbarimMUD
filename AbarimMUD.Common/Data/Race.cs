using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum Race
	{
		Human,
		Dwarf,
		Elf,
		Gnome,
		Dragon,
		Orc,
		Undead,
		Skeleton,
		Unique,
		Null,
		Lich,
		Ghoul,
		Snake,
		Rabbit,
		SongBird,
		Bear,
		Wyvern,
		Cat,
		Dog,
		Wolf,
		Lizard,
		Serpent,
		DarkElf,
		Trolloc,
		Centipede,
		Bat,
		Giant,
		Troll,
		Goblin,
		Doll,
		Centaur,
		Fox,
		Fido,
		Waterfowl,
		Kobold,
		Hobgoblin,
		Schoolmonster,
		Pig,
		Nymph,
		Modron,
		Duergar,
		Fish,
		Rat,
		Bird,
		Angel,
		Pixie,

	}

	public class RaceInfo
	{
		public bool PcRace { get; }
		public HashSet<MobileFlags> MobileFlags { get; } = new HashSet<MobileFlags>();
		public HashSet<AffectedByFlags> AffectedByFlags { get; } = new HashSet<AffectedByFlags>();
		public HashSet<ResistanceFlags> ImmuneFlags { get; } = new HashSet<ResistanceFlags>();
		public HashSet<ResistanceFlags> ResistanceFlags { get; } = new HashSet<ResistanceFlags>();
		public HashSet<ResistanceFlags> VulnerableFlags { get; } = new HashSet<ResistanceFlags>();
		public FormFlags FormFlags { get; }
		public PartFlags PartFlags { get; }

		public RaceInfo(bool pcRace, 
			HashSet<MobileFlags> mobileFlags, HashSet<AffectedByFlags> affectedByFlags,
			HashSet<ResistanceFlags> immuneFlags, HashSet<ResistanceFlags> resistanceFlags, 
			HashSet<ResistanceFlags> vulnerableFlags, FormFlags formFlags, PartFlags partFlags)
		{
			PcRace = pcRace;
			MobileFlags = mobileFlags;
			AffectedByFlags = affectedByFlags;
			ImmuneFlags = immuneFlags;
			ResistanceFlags = resistanceFlags;
			VulnerableFlags = vulnerableFlags;
			FormFlags = formFlags;
			PartFlags = partFlags;
		}
	}

/*	public static class RaceExtensions
	{
		private static readonly Dictionary<Race, RaceInfo> _raceData = new Dictionary<Race, RaceInfo>
		{
			[Race.Unique] = new RaceInfo(false, 0, 0, 0, 0, 0, 0, 0, 0),
			[Race.Human] = new RaceInfo(true, 0, 0, 0, 0, 0, 0, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Elf] = new RaceInfo(true, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Charm, ResistanceFlags.Iron,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Dwarf] = new RaceInfo(true, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Poison | ResistanceFlags.Disease,
				ResistanceFlags.Drowning, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Giant] = new RaceInfo(true, 0, 0, 0, 0, ResistanceFlags.Fire | ResistanceFlags.Cold,
				ResistanceFlags.Mental | ResistanceFlags.Lightning, FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Pixie] = new RaceInfo(false, 0,
				AffectedByFlags.Flying | AffectedByFlags.DetectGood | AffectedByFlags.DetectEvil | AffectedByFlags.DetectMagic,
				0, 0, 0, 0, FormFlags.FormsHumanoid | FormFlags.Magical, PartFlags.PartsHumanoid | PartFlags.Wings),
			[Race.Bat] = new RaceInfo(false, 0,
				AffectedByFlags.Flying | AffectedByFlags.DarkVision, MobileOffensiveFlags.Dodge | MobileOffensiveFlags.Fast,
				0, 0, ResistanceFlags.Light, FormFlags.Mammal, PartFlags.PartsQuadRuped | PartFlags.Wings),
			[Race.Bear] = new RaceInfo(false, 0, 0, MobileOffensiveFlags.Crush | MobileOffensiveFlags.Disarm | MobileOffensiveFlags.Berserk, 0,
				ResistanceFlags.Bash | ResistanceFlags.Cold, 0, FormFlags.Mammal, PartFlags.PartsBiped | PartFlags.Claws | PartFlags.Fangs),
			[Race.Cat] = new RaceInfo(false, 0, AffectedByFlags.DarkVision, MobileOffensiveFlags.Fast | MobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsFeline),
			[Race.Centipede] = new RaceInfo(false, 0, AffectedByFlags.DarkVision, 0, 0,
				ResistanceFlags.Pierce | ResistanceFlags.Cold, ResistanceFlags.Bash,
				FormFlags.FormsBug | FormFlags.Poison, PartFlags.Head | PartFlags.Legs | PartFlags.Eye),
			[Race.Dog] = new RaceInfo(false, 0, 0, MobileOffensiveFlags.Fast, 0, 0, 0, FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Claws),
			[Race.Doll] = new RaceInfo(false, 0, 0, 0, ResistanceFlags.Cold | ResistanceFlags.Poison | ResistanceFlags.Holy |
				ResistanceFlags.Negative | ResistanceFlags.Mental | ResistanceFlags.Disease | ResistanceFlags.Drowning,
				ResistanceFlags.Bash | ResistanceFlags.Light, ResistanceFlags.Slash | ResistanceFlags.Fire |
				ResistanceFlags.Acid | ResistanceFlags.Lightning | ResistanceFlags.Energy,
				FormFlags.Other | FormFlags.Construct | FormFlags.Biped | FormFlags.ColdBlood,
				PartFlags.PartsHumanoid & ~(PartFlags.PartsAlive | PartFlags.Ear)),
			[Race.Dragon] = new RaceInfo(false, 0, AffectedByFlags.Infrared | AffectedByFlags.Flying, 0, 0,
				ResistanceFlags.Fire | ResistanceFlags.Bash | ResistanceFlags.Charm, ResistanceFlags.Pierce | ResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Sentinent | FormFlags.Dragon,
				PartFlags.PartsLizard | PartFlags.Fingers | PartFlags.Claws | PartFlags.Fangs),
			[Race.Fido] = new RaceInfo(false, 0, 0, MobileOffensiveFlags.Dodge | MobileOffensiveFlags.AssistRace, 0, 0,
				ResistanceFlags.Magic, FormFlags.Mammal | FormFlags.Poison, PartFlags.PartsCanine | PartFlags.Tail),
			[Race.Fox] = new RaceInfo(false, 0, AffectedByFlags.DarkVision, MobileOffensiveFlags.Fast | MobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Tail),
			[Race.Goblin] = new RaceInfo(false, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Disease, ResistanceFlags.Magic,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Hobgoblin] = new RaceInfo(false, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Disease | ResistanceFlags.Poison, 0,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid | PartFlags.Tusks),
			[Race.Kobold] = new RaceInfo(false, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Poison, ResistanceFlags.Magic,
				FormFlags.FormsHumanoid | FormFlags.Poison, PartFlags.PartsHumanoid | PartFlags.Tail),
			[Race.Lizard] = new RaceInfo(false, 0, 0, 0, 0, ResistanceFlags.Poison, ResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Animal | FormFlags.Reptile | FormFlags.ColdBlood, PartFlags.PartsLizard),
			[Race.Modron] = new RaceInfo(false, 0, AffectedByFlags.Infrared, MobileOffensiveFlags.AssistRace | MobileOffensiveFlags.AssistAlign,
				ResistanceFlags.Charm | ResistanceFlags.Disease | ResistanceFlags.Mental | ResistanceFlags.Holy | ResistanceFlags.Negative,
				ResistanceFlags.Fire | ResistanceFlags.Cold | ResistanceFlags.Acid, 0,
				FormFlags.Sentinent, PartFlags.PartsHumanoid & ~(PartFlags.PartsAlive | PartFlags.Fingers)),
			[Race.Orc] = new RaceInfo(false, 0, AffectedByFlags.Infrared, 0, 0, ResistanceFlags.Disease, ResistanceFlags.Light,
				FormFlags.FormsHumanoid, PartFlags.PartsHumanoid),
			[Race.Pig] = new RaceInfo(false, 0, 0, 0, 0, 0, 0, FormFlags.Mammal, PartFlags.PartsQuadRuped),
			[Race.Rabbit] = new RaceInfo(false, 0, 0, MobileOffensiveFlags.Dodge | MobileOffensiveFlags.Fast, 0, 0, 0, FormFlags.Mammal,
				PartFlags.PartsQuadRuped),
			[Race.SchoolMonster] = new RaceInfo(false, MobileFlags.NoAlign, 0, 0, ResistanceFlags.Charm | ResistanceFlags.Summon, 0,
				ResistanceFlags.Magic, FormFlags.Edible | FormFlags.Biped | FormFlags.Mammal,
				PartFlags.PartsBiped | PartFlags.Tail | PartFlags.Claws),
			[Race.Snake] = new RaceInfo(false, 0, 0, 0, 0, ResistanceFlags.Poison, ResistanceFlags.Cold,
				FormFlags.Edible | FormFlags.Animal | FormFlags.Reptile | FormFlags.Snake | FormFlags.ColdBlood,
				PartFlags.PartsReptile | PartFlags.Fangs),
			[Race.SongBird] = new RaceInfo(false, 0, AffectedByFlags.Flying, MobileOffensiveFlags.Dodge | MobileOffensiveFlags.Fast, 0, 0, 0,
				FormFlags.Bird, PartFlags.PartsBird),
			[Race.Troll] = new RaceInfo(false, 0, AffectedByFlags.Regeneration | AffectedByFlags.Infrared | AffectedByFlags.DetectHidden,
				MobileOffensiveFlags.Berserk, 0, ResistanceFlags.Charm | ResistanceFlags.Bash, ResistanceFlags.Fire | ResistanceFlags.Acid,
				FormFlags.FormsHumanoid | FormFlags.Poison, PartFlags.PartsHumanoid | PartFlags.Claws | PartFlags.Fangs),
			[Race.WaterFowl] = new RaceInfo(false, 0, AffectedByFlags.Swim | AffectedByFlags.Flying, 0, 0, ResistanceFlags.Drowning, 0,
				FormFlags.Bird, PartFlags.PartsBird),
			[Race.Wolf] = new RaceInfo(false, 0, AffectedByFlags.DarkVision, MobileOffensiveFlags.Fast | MobileOffensiveFlags.Dodge, 0, 0, 0,
				FormFlags.Mammal, PartFlags.PartsCanine | PartFlags.Claws | PartFlags.Tail),
			[Race.Wyvern] = new RaceInfo(false, 0,
				AffectedByFlags.Flying | AffectedByFlags.DetectInvis | AffectedByFlags.DetectHidden,
				MobileOffensiveFlags.Bash | MobileOffensiveFlags.Fast | MobileOffensiveFlags.Dodge,
				ResistanceFlags.Poison, 0, ResistanceFlags.Light,
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
	}*/
}
