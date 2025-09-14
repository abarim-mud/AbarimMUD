using AbarimMUD.Attributes;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

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
		NoWander,
		Bash,
		Berserk,
		Dodge,
		Kick,
		Parry,
		Trip,
		Aggressive,
		Undead,
		Wimpy,
		Healer,
		Scavenger,
		Banker,
		NoPurge,
		NoAlign,
		Pet,
		Fade,
		Changer,
		Rescue,
		Outdoors,
		Indoors,
		NoTrack,
		UpdateAlways,
		Crush,
		Fast,
		Tail,
		AssistRace,
		AssistAlign,
		AssistPlayer,
		AssistAll,
		AssistGuard,
		AssistId,
		Enchanter,

		AssistPlayers = AssistPlayer,
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
		Shock,
		Bludgeon,
		Rake,
		Beat,
		Zap
	}

	public class Mobile : AreaEntity
	{
		public class MobileData
		{
			public int Id { get; set; }
			public MobileTemplate Template { get; set; }
			public HashSet<string> Keywords { get; set; }
			public string ShortDescription { get; set; }
			public string LongDescription { get; set; }
			public string Description { get; set; }
			public int? Hitpoints { get; set; }
			public int? Mana { get; set; }
			public int? Moves { get; set; }
			public int? Armor { get; set; }
			public Attack[] Attacks { get; set; }
			public int? Level { get; set; }
			public int? Gold { get; set; }
			public Sex? Sex { get; set; }

			public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();
			public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

			public List<LootRecord> Loot { get; set; } = new List<LootRecord>();

			public Shop Shop { get; set; }

			public PlayerClass Guildmaster { get; set; }

			public ForgeShop ForgeShop { get; set; }

			public ExchangeShop ExchangeShop { get; set; }
		}

		public override int Id
		{
			get => Data.Id;
			set => Data.Id = value;
		}

		public MobileTemplate Template
		{
			get => Data.Template;
			set => Data.Template = value;
		}

		public MobileData Data { get; set; }

		[Browsable(false)]
		public HashSet<string> Keywords
		{
			get
			{
				if (Data.Keywords.Count == 0 && Template != null)
				{
					return Template.Keywords;
				}

				return Data.Keywords;
			}

			set => Data.Keywords = value;
		}

		[OLCAlias("short")]
		public string ShortDescription
		{
			get
			{
				if (string.IsNullOrEmpty(Data.ShortDescription) && Template != null)
				{
					return Template.ShortDescription;
				}

				return Data.ShortDescription;
			}

			set => Data.ShortDescription = value;
		}

		[OLCAlias("long")]
		public string LongDescription
		{
			get
			{
				if (string.IsNullOrEmpty(Data.LongDescription) && Template != null)
				{
					return Template.LongDescription;
				}

				return Data.LongDescription;
			}

			set => Data.LongDescription = value;
		}

		public string Description
		{
			get
			{
				if (string.IsNullOrEmpty(Data.Description) && Template != null)
				{
					return Template.Description;
				}

				return Data.Description;
			}

			set => Data.Description = value;
		}

		public int Level
		{
			get
			{
				if (Data.Level == null && Template != null)
				{
					return Template.Level;
				}

				EnsureSet(Data.Level, "Level");

				return Data.Level.Value;
			}

			set
			{
				if (value == Data.Level)
				{
					return;
				}

				Data.Level = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Gold
		{
			get
			{
				if (Data.Gold == null && Template != null)
				{
					return Template.Gold;
				}

				EnsureSet(Data.Gold, "Gold");

				return Data.Gold.Value;
			}


			set
			{
				if (value == Data.Gold)
				{
					return;
				}

				Data.Gold = value;
				InvalidateCreaturesOfThisClass();
			}
		}


		public Sex Sex
		{
			get
			{
				if (Data.Sex == null && Template != null)
				{
					return Template.Sex;
				}

				EnsureSet(Data.Sex, "Sex");

				return Data.Sex.Value;
			}

			set
			{
				if (value == Data.Sex)
				{
					return;
				}

				Data.Sex = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Hitpoints
		{
			get
			{
				if (Data.Hitpoints == null && Template != null)
				{
					return Template.Hitpoints;
				}

				EnsureSet(Data.Hitpoints, "Hitpoints");

				return Data.Hitpoints.Value;
			}

			set
			{
				if (value == Data.Hitpoints)
				{
					return;
				}

				Data.Hitpoints = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Mana
		{
			get
			{
				if (Data.Mana == null && Template != null)
				{
					return Template.Mana;
				}

				EnsureSet(Data.Mana, "Mana");

				return Data.Mana.Value;
			}

			set
			{
				if (value == Data.Mana)
				{
					return;
				}

				Data.Mana = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Moves
		{
			get
			{
				if (Data.Moves == null && Template != null)
				{
					return Template.Moves;
				}

				EnsureSet(Data.Moves, "Moves");

				return Data.Moves.Value;
			}

			set
			{
				if (value == Data.Moves)
				{
					return;
				}

				Data.Moves = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Armor
		{
			get
			{
				if (Data.Armor == null && Template != null)
				{
					return Template.Armor;
				}

				EnsureSet(Data.Armor, "Armor");

				return Data.Armor.Value;
			}

			set
			{
				if (value == Data.Armor)
				{
					return;
				}

				Data.Armor = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public Attack[] Attacks
		{
			get
			{
				if (Data.Attacks == null && Template != null)
				{
					return Template.Attacks;
				}

				return Data.Attacks;
			}

			set
			{
				Data.Attacks = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[Browsable(false)]
		public HashSet<MobileFlags> Flags
		{
			get
			{
				if (Template != null)
				{
					return Utility.MergeGet(Template.Flags, Data.Flags);
				}

				return Data.Flags;
			}

			set
			{
				if (Template != null)
				{
					Data.Flags = Utility.MergeSet(Template.Flags, value);
				}
				else
				{
					Data.Flags = value;
				}
			}
		}

		public List<LootRecord> Loot
		{
			get
			{
				if (Template != null)
				{
					return Utility.MergeGet(Template.Loot, Data.Loot);
				}

				return Data.Loot;
			}
		}

		[Browsable(false)]
		public PlayerClass Guildmaster
		{
			get => Data.Guildmaster;
			set => Data.Guildmaster = value;
		}

		[Browsable(false)]
		public List<MobileSpecialAttack> SpecialAttacks => Data.SpecialAttacks;

		public Shop Shop
		{
			get => Data.Shop;

			set
			{
				if (value == Data.Shop)
				{
					return;
				}

				Data.Shop = value;

				// Rebuild inventories
				foreach (var creature in Creature.ActiveCreatures)
				{
					var asMobile = creature as MobileInstance;
					if (asMobile == null)
					{
						continue;
					}

					if (asMobile.Info.Id == Id)
					{
						asMobile.RebuildInventory();
					}
				}
			}
		}

		public ForgeShop ForgeShop
		{
			get => Data.ForgeShop;
			set => Data.ForgeShop = value;
		}

		public ExchangeShop ExchangeShop
		{
			get => Data.ExchangeShop;
			set => Data.ExchangeShop = value;
		}

		public Mobile()
		{
			Data = new MobileData();
		}

		public Mobile(MobileData data)
		{
			Data = data ?? throw new ArgumentNullException(nameof(data));
		}

		private void EnsureSet<T>(T? value, string prop) where T : struct
		{
			if (value == null)
			{
				throw new Exception($"{ShortDescription}'s {prop} is not set.");
			}
		}

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public CreatureStats CreateStats()
		{
			var stats = new CreatureStats
			{
				HitpointsBase = Hitpoints,
				ManaBase = Mana,
				MovesBase = Moves,
				Armor = Armor
			};

			stats.Attacks.AddRange(Attacks);

			return stats;
		}

		public Mobile Clone()
		{
			var clone = new Mobile
			{
				Id = Id,
				Area = Area,
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Level = Level,
				Sex = Sex,
				Hitpoints = Hitpoints,
				Mana = Mana,
				Moves = Moves,
				Armor = Armor,
				Attacks = Attacks,
				Guildmaster = Guildmaster,
				Shop = Shop,
				ForgeShop = ForgeShop,
				Gold = Gold,
			};

			foreach (var word in Keywords)
			{
				clone.Keywords.Add(word);
			}

			foreach (var lootRec in Loot)
			{
				clone.Loot.Add(lootRec.Clone());
			}

			foreach (var flag in Flags)
			{
				clone.Flags.Add(flag);
			}

			foreach (var atk in SpecialAttacks)
			{
				clone.SpecialAttacks.Add(atk.Clone());
			}

			return clone;
		}

		private void InvalidateCreaturesOfThisClass()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public static Mobile GetMobileById(int id) => Area.Storage.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Area.Storage.EnsureMobileById(id);
	}

	public static class MobileExtensions
	{
		private class AttackNames
		{
			public string Verb;

			public AttackNames(string verb)
			{
				Verb = verb;
			}
		}

		private static readonly AttackNames[] _attackNames = new AttackNames[Enum.GetNames(typeof(AttackType)).Length];

		static MobileExtensions()
		{
			_attackNames[(int)AttackType.Hit] = new AttackNames("hits");
			_attackNames[(int)AttackType.Slice] = new AttackNames("slices");
			_attackNames[(int)AttackType.Stab] = new AttackNames("stabs");
			_attackNames[(int)AttackType.Slash] = new AttackNames("slashes");
			_attackNames[(int)AttackType.Whip] = new AttackNames("whips");
			_attackNames[(int)AttackType.Claw] = new AttackNames("claws");
			_attackNames[(int)AttackType.Hack] = new AttackNames("hacks");
			_attackNames[(int)AttackType.Blast] = new AttackNames("blasts");
			_attackNames[(int)AttackType.Pound] = new AttackNames("pounds");
			_attackNames[(int)AttackType.Crush] = new AttackNames("crushes");
			_attackNames[(int)AttackType.Grep] = new AttackNames("greps");
			_attackNames[(int)AttackType.Bite] = new AttackNames("bites");
			_attackNames[(int)AttackType.Pierce] = new AttackNames("pierces");
			_attackNames[(int)AttackType.Suction] = new AttackNames("suctions");
			_attackNames[(int)AttackType.Beating] = new AttackNames("beats");
			_attackNames[(int)AttackType.Charge] = new AttackNames("charges");
			_attackNames[(int)AttackType.Slap] = new AttackNames("slaps");
			_attackNames[(int)AttackType.Punch] = new AttackNames("punches");
			_attackNames[(int)AttackType.Cleave] = new AttackNames("cleaves");
			_attackNames[(int)AttackType.Scratch] = new AttackNames("scratches");
			_attackNames[(int)AttackType.Peck] = new AttackNames("pecks");
			_attackNames[(int)AttackType.Chop] = new AttackNames("chops");
			_attackNames[(int)AttackType.Sting] = new AttackNames("stings");
			_attackNames[(int)AttackType.Smash] = new AttackNames("smashes");
			_attackNames[(int)AttackType.Chomp] = new AttackNames("chomps");
			_attackNames[(int)AttackType.Thrust] = new AttackNames("thrusts");
			_attackNames[(int)AttackType.Slime] = new AttackNames("slimes");
			_attackNames[(int)AttackType.Shock] = new AttackNames("shocks");
			_attackNames[(int)AttackType.Bludgeon] = new AttackNames("bludgeons");
			_attackNames[(int)AttackType.Rake] = new AttackNames("rakes");
			_attackNames[(int)AttackType.Beat] = new AttackNames("beats");
			_attackNames[(int)AttackType.Zap] = new AttackNames("zaps");
		}

		public static string GetAttackNoun(this AttackType attackType)
		{
			return attackType.ToString().ToLower();
		}

		public static string GetAttackVerb(this AttackType attackType)
		{
			return _attackNames[(int)attackType].Verb;
		}
	}
}