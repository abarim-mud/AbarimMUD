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

	public class MobileLoot
	{
		public Item Item { get; set; }
		public int ProbabilityPercentage { get; set; }

		public MobileLoot Clone() => new MobileLoot
		{
			Item = Item,
			ProbabilityPercentage = ProbabilityPercentage
		};
	}

	public class Mobile : AreaEntity
	{
		public const AttackType DefaultAttackType = AttackType.Hit;

		public const int DefaultPenetration = 0;
		public const int DefaultArmor = 0;
		public const int DefaultGold = 100;
		public const int DefaultHitpoints = 100;
		public const int DefaultMana = 100;
		public const int DefaultMoves = 1000;
		public static readonly ValueRange DefaultDamageRange = new ValueRange(1, 4);


		private int _hitpoints = DefaultHitpoints, _mana = DefaultMana, _moves = DefaultMoves;
		private int _armor = DefaultArmor;
		private int _attacksCount = 1;
		private AttackType _attackType = AttackType.Hit;
		private int _penetration = DefaultPenetration;
		private ValueRange _damageRange = DefaultDamageRange;
		private int _level;
		private int _gold = DefaultGold;

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

		public int Level
		{
			get => _level;

			set
			{
				if (value == _level)
				{
					return;
				}

				_level = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Gold
		{
			get => _gold;

			set
			{
				if (value == _gold)
				{
					return;
				}

				_gold = value;
				InvalidateCreaturesOfThisClass();
			}
		}


		public Sex Sex { get; set; }

		public int Hitpoints
		{
			get => _hitpoints;

			set
			{
				if (value == _hitpoints)
				{
					return;
				}

				_hitpoints = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Mana
		{
			get => _mana;

			set
			{
				if (value == _mana)
				{
					return;
				}

				_mana = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Moves
		{
			get => _moves;

			set
			{
				if (value == _moves)
				{
					return;
				}

				_moves = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Armor
		{
			get => _armor;

			set
			{
				if (value == _armor)
				{
					return;
				}

				_armor = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int AttacksCount
		{
			get => _attacksCount;

			set
			{
				if (value < 1)
				{
					value = 1;
				}

				if (value == _attacksCount)
				{
					return;
				}

				_attacksCount = value;
				InvalidateCreaturesOfThisClass();
			}
		}


		public AttackType AttackType
		{
			get => _attackType;

			set
			{
				if (value == _attackType)
				{
					return;
				}

				_attackType = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int Penetration
		{
			get => _penetration;

			set
			{
				if (value == _penetration)
				{
					return;
				}

				_penetration = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange DamageRange
		{
			get => _damageRange;

			set
			{
				if (value == _damageRange)
				{
					return;
				}

				_damageRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public Dictionary<string, MobileLoot> Loot { get; set; } = new Dictionary<string, MobileLoot>();

		[Browsable(false)]
		public PlayerClass Guildmaster { get; set; }

		[Browsable(false)]
		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

		[Browsable(false)]
		public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();

		public StockItemType? Shop { get; set; }


		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public CreatureStats CreateStats()
		{
			var stats = new CreatureStats
			{
				MaxHitpoints = Hitpoints,
				MaxMana = Mana,
				MaxMoves = Moves,
				Armor = Armor
			};

			// Calculate attacks' values
			for (var i = 0; i < AttacksCount; ++i)
			{
				var attack = new Attack(AttackType, Penetration, DamageRange);
				stats.Attacks.Add(attack);
			}

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
				AttacksCount = AttacksCount,
				AttackType = AttackType,
				Penetration = Penetration,
				DamageRange = DamageRange,
				Guildmaster = Guildmaster,
				Shop = Shop,
				Gold = Gold,
			};

			foreach (var word in Keywords)
			{
				clone.Keywords.Add(word);
			}

			foreach (var pair in Loot)
			{
				clone.Loot[pair.Key] = pair.Value.Clone();
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