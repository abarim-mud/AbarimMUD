namespace AbarimMUD.Common.Data
{
	public class RaceInfo
	{
		public bool PcRace { get; }
		public MobileFlags MobileFlags { get; }
		public AffectedByFlags AffectedByFlags { get; }
		public MobileOffensiveFlags OffensiveFlags { get; }
		public ResistanceFlags ImmuneFlags { get; }
		public ResistanceFlags ResistanceFlags { get; }
		public ResistanceFlags VulnerableFlags { get; }
		public FormFlags FormFlags { get; }
		public PartFlags PartFlags { get; }

		public RaceInfo(bool pcRace, MobileFlags mobileFlags, AffectedByFlags affectedByFlags,
			MobileOffensiveFlags offensiveFlags, ResistanceFlags immuneFlags, ResistanceFlags resistanceFlags,
			ResistanceFlags vulnerableFlags, FormFlags formFlags, PartFlags partFlags)
		{
			PcRace = pcRace;
			MobileFlags = mobileFlags;
			AffectedByFlags = affectedByFlags;
			OffensiveFlags = offensiveFlags;
			ImmuneFlags = immuneFlags;
			ResistanceFlags = resistanceFlags;
			VulnerableFlags = vulnerableFlags;
			FormFlags = formFlags;
			PartFlags = partFlags;
		}
	}
}
