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

	public class Mobile : AreaEntity
	{
		public const int DefaultArmor = 0;
		public const int DefaultGold = 100;
		public const int DefaultHitpoints = 100;
		public const int DefaultMana = 100;
		public const int DefaultMoves = 1000;

		private static readonly ValueRange AutoLevelArmor = new ValueRange(10, 250);
		private static readonly ValueRange AutoLevelGold = new ValueRange(100, 20000);
		private static readonly ValueRange AutoLevelHitpoints = new ValueRange(100, 5000);
		private static readonly ValueRange AutoLevelMana = new ValueRange(100, 1000);
		private static readonly ValueRange AutoLevelAttackBonus = new ValueRange(10, 250);
		private static readonly ValueRange AutoLevelMinimumDamage = new ValueRange(4, 50);
		private static readonly ValueRange AutoLevelMaximumDamage = new ValueRange(8, 120);
		private static readonly int[] AutoLevelAttacks = new int[] { 3, 15, 30, 45, 60, 80, 100 };

		private int _hitpoints = DefaultHitpoints, _mana = DefaultMana, _moves = DefaultMoves;
		private int _armor = DefaultArmor;
		private int _level;
		private int _gold = DefaultGold;
		private Shop _shop;

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

		public MobileSpecialAttack[] SpecialAttacks { get; set; }

		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

		public List<LootRecord> Loot { get; set; } = new List<LootRecord>();

		public PlayerClass Guildmaster { get; set; }

		public Shop Shop
		{
			get => _shop;

			set
			{
				if (value == _shop)
				{
					return;
				}

				_shop = value;

				// Rebuild inventories
				RebuildInventoriesOfThisTemplate();
			}
		}

		public ForgeShop ForgeShop { get; set; }

		public ExchangeShop ExchangeShop { get; set; }

		public long Experience { get; set; }

		public Mobile()
		{
		}

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


		public long CalculateXpAward() => CreatureStats.CalculateXpAward(Hitpoints, Armor, Attacks);

		private bool IsMobileOfThisTemplate(MobileInstance mobile)
		{
			return mobile.Info.Id == Id;
		}

		private void InvalidateCreaturesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.IsMobileOfThisTemplate(asMobile))
				{
					creature.InvalidateStats();
				}
			}
		}

		private void RebuildInventoriesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.IsMobileOfThisTemplate(asMobile))
				{
					asMobile.RebuildInventory();
				}
			}
		}

		public Mobile CloneMobile()
		{
			var clone = new Mobile
			{
				Id = Id,
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Level = Level,
				Sex = Sex,
				Hitpoints = Hitpoints,
				Mana = Mana,
				Moves = Moves,
				Armor = Armor,
				Gold = Gold,
			};

			clone.Attacks = new Attack[Attacks.Length];
			for (var i = 0; i < clone.Attacks.Length; ++i)
			{
				clone.Attacks[i] = Attacks[i].Clone();
			}

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

			if (SpecialAttacks != null && SpecialAttacks.Length > 0)
			{
				clone.SpecialAttacks = new MobileSpecialAttack[SpecialAttacks.Length];

				for (var i = 0; i < clone.SpecialAttacks.Length; ++i)
				{
					clone.SpecialAttacks[i] = SpecialAttacks[i].Clone();
				}
			}

			return clone;
		}

		/// <summary>
		/// Sets the mobile parameters based on the level.
		/// The level is set too.
		/// </summary>
		/// <param name="level"></param>
		/// <param name=""></param>
		public void SetAutoLevel(int level, AttackType attackType = AttackType.Hit)
		{
			Level = level;

			Armor = AutoLevelArmor.GetValueByLevel(level).RoundDownToNearest(10);
			Gold = AutoLevelGold.GetValueByLevel(level).RoundDownToNearest(100);

			if (Gold > 1000)
			{
				if (Gold < 3000)
				{
					Gold = Gold.RoundUpToNearest(500);
				} else if (Gold < 10000)
				{
					Gold = Gold.RoundUpToNearest(1000);
				} else
				{
					Gold = Gold.RoundUpToNearest(5000);
				}
			}

			Hitpoints = AutoLevelHitpoints.GetValueByLevel(level).RoundDownToNearest(100);
			Mana = AutoLevelMana.GetValueByLevel(level).RoundDownToNearest(100);

			var attackBonus = AutoLevelAttackBonus.GetValueByLevel(level).RoundDownToNearest(10);

			var attacksCount = 1;
			for (var i = 0; i < AutoLevelAttacks.Length; ++i)
			{
				var lvl = AutoLevelAttacks[i];
				if (level < lvl)
				{
					break;
				}

				++attacksCount;
			}

			var damage = new ValueRange(AutoLevelMinimumDamage.GetValueByLevel(level), AutoLevelMaximumDamage.GetValueByLevel(level));

			var attacks = new List<Attack>();
			for (var i = 0; i < attacksCount; ++i)
			{
				attacks.Add(new Attack(attackType, attackBonus, damage));
			}

			Attacks = attacks.ToArray();
		}

		public object Clone() => CloneMobile();

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public static Mobile GetMobileById(int id) => Area.Storage.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Area.Storage.EnsureMobileById(id);
	}
}
