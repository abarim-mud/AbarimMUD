using AbarimMUD.Attributes;
using AbarimMUD.Storage;
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

	public class Mobile : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Mobile> Storage = new Mobiles();

		public const int DefaultArmor = 0;
		public const int DefaultGold = 100;
		public const int DefaultHitpoints = 100;
		public const int DefaultMana = 100;
		public const int DefaultMoves = 1000;

		private int _hitpoints = DefaultHitpoints, _mana = DefaultMana, _moves = DefaultMoves;
		private int _armor = DefaultArmor;
		private int _level;
		private int _gold = DefaultGold;

		public string Id { get; set; }

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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public Attack[] Attacks { get; set; }

		[Browsable(false)]
		public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();

		[Browsable(false)]
		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

		[Browsable(false)]
		public List<LootRecord> Loot { get; set; } = new List<LootRecord>();

		public Mobile()
		{
		}

		private void InvalidateCreaturesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null || asMobile.Info.Template == null)
				{
					continue;
				}

				if (asMobile.Info.Template.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Mobile GetMobileById(string id) => Storage.GetByKey(id);
		public static Mobile EnsureMobileById(string id) => Storage.EnsureByKey(id);

	}
}
