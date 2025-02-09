using AbarimMUD.Attributes;
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

	public class Mobile : AreaEntity
	{
		private MobileClass _class;
		private int _level;

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

		[Browsable(false)]
		public MobileClass Class
		{
			get => _class;

			set
			{
				if (value == _class)
				{
					return;
				}

				_class = value;
				Creature.InvalidateMobiles(this);
			}
		}

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
				Creature.InvalidateMobiles(this);
			}
		}

		public Sex Sex { get; set; }

		[Browsable(false)]
		public PlayerClass Guildmaster { get; set; }

		[Browsable(false)]
		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();
		
		[Browsable(false)]
		public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();

		public StockItemType? Shop { get; set; }

		public int Gold { get; set; }

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

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