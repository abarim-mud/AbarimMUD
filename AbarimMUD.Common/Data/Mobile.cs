using AbarimMUD.Attributes;
using AbarimMUD.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AbarimMUD.Data
{
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
		private MobileClass _class;
		private int _level;
		private Shop _shop;
		private int _hitpoints;
		private int? _mana, _moves;
		private int _armor;
		private Attack[] _attacks;

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

		public Sex Sex { get; set; }

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
				InvalidateCreaturesOfThisTemplate();
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
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public int Gold { get; set; }

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

		public int? Mana
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

		public int? Moves
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

		public Attack[] Attacks
		{
			get
			{
				if (_attacks == null)
				{
					_attacks = new Attack[1];
					_attacks[0] = new Attack(AttackType.Hit, 10, new ValueRange(4, 8));
				}

				return _attacks;
			}

			set
			{
				if (value == null || value.Length < 1)
				{
					throw new ArgumentNullException(nameof(value));
				}

				_attacks = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		private int HolyResistance
		{
			get
			{
				if (Class.HolyResistance != null)
				{
					return Class.HolyResistance.Value.GetValue(Level);
				}

				return Configuration.DefaultHolyResistance;
			}
		}

		private int FireResistance
		{
			get
			{
				if (Class.FireResistance != null)
				{
					return Class.FireResistance.Value.GetValue(Level);
				}

				return Configuration.DefaultFireResistance;
			}
		}

		private int ColdResistance
		{
			get
			{
				if (Class.ColdResistance != null)
				{
					return Class.ColdResistance.Value.GetValue(Level);
				}

				return Configuration.DefaultColdResistance;
			}
		}

		private int ShockResistance
		{
			get
			{
				if (Class.ShockResistance != null)
				{
					return Class.ShockResistance.Value.GetValue(Level);
				}

				return Configuration.DefaultShockResistance;
			}
		}


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

		[OLCIgnore]
		public long Experience { get; set; }

		public Mobile()
		{
		}

		public CreatureStats CreateStats()
		{
			var stats = new CreatureStats
			{
				HitpointsBase = Hitpoints,
				ManaBase = Mana ?? Configuration.DefaultMobileMana,
				MovesBase = Moves ?? Configuration.DefaultMobileMoves,
				Armor = Armor,
				HolyResistance = HolyResistance,
				FireResistance = FireResistance,
				ColdResistance = ColdResistance,
				ShockResistance = ShockResistance,
				IsEthereal = Class.IsEthereal,
			};

			stats.Attacks.AddRange(Attacks);

			return stats;
		}


		public long CalculateXpAward()
		{
			var result = CreatureStats.CalculateXpAward(Hitpoints, Armor, Attacks);

			var addPerc = 0;
			if (Class.IsEthereal)
			{
				addPerc += 20;
			}

			if (addPerc > 0)
			{
				var add = (long)(result * addPerc / 100.0f);
				result += add;
			}

			return result;
		}

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
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Level = Level,
				Class = Class,
				Gold = Gold,
				Hitpoints = Hitpoints,
				Mana = Mana,
				Moves = Moves,
				Armor = Armor,
				Attacks = Attacks.CloneAttacks(),
				Sex = Sex
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

			return clone;
		}

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public static Mobile GetMobileById(int id) => Area.Storage.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Area.Storage.EnsureMobileById(id);
	}
}
