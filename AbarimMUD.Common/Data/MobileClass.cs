using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.StorageAPI;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Data
{
	public class AttackInfo
	{
		public int MinimumLevel { get; set; }
		public bool IsDefaultAttack { get; set; } = true;
		public AttackType AttackType { get; set; }
		public ValueRange PenetrationRange { get; set; }
		public ValueRange MinimumDamageRange { get; set; }
		public ValueRange MaximumDamageRange { get; set; }

		public AttackInfo()
		{
		}

		public AttackInfo(int minimumLevel, AttackType attackType, ValueRange penetration,
			ValueRange minimumDamageRange, ValueRange maximumDamageRange)
		{
			MinimumLevel = minimumLevel;
			IsDefaultAttack = false;
			AttackType = attackType;
			PenetrationRange = penetration;
			MinimumDamageRange = minimumDamageRange;
			MaximumDamageRange = maximumDamageRange;
		}
	}

	public class MobileLoot
	{
		public int MaximumLevel { get; set; }
		public Item[] Items { get; set; }
	}

	public class MobileClass : IStoredInFile, ISerializationEvents, ICloneable
	{
		public const AttackType DefaultAttackType = Data.AttackType.Hit;
		public const int DefaultPenetration = 0;

		public static readonly ValueRange DefaultDamageRange = new ValueRange(1, 4);
		public static readonly ValueRange DefaultHitpoints = new ValueRange(1, 100);
		public static readonly ValueRange DefaultMana = new ValueRange(100, 200);
		public static readonly ValueRange DefaultMoves = new ValueRange(100, 200);
		public static readonly ValueRange DefaultArmor = new ValueRange(0, 0);
		public static readonly ValueRange DefaultGold = new ValueRange(0, 1000);
		public static readonly MultipleFilesStorage<MobileClass> Storage = new MobileClasses();

		private AttackType _attackType = Data.AttackType.Hit;
		private ValueRange _hitpointsRange = DefaultHitpoints, _manaRange = DefaultMana, _movesRange = DefaultMoves;
		private ValueRange _armorRange = DefaultArmor;
		private ValueRange _penetrationRange;
		private ValueRange _minimumDamageRange = DefaultDamageRange;
		private ValueRange _maximumDamageRange = DefaultDamageRange;
		private ValueRange _goldRange = DefaultGold;
		private AttackInfo[] _attacks;
		private MobileLoot[] _loot;

		[OLCIgnore]
		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }

		[OLCAlias("hprange")]
		public ValueRange HitpointsRange
		{
			get => _hitpointsRange;

			set
			{
				if (value == _hitpointsRange)
				{
					return;
				}

				_hitpointsRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("manarange")]
		public ValueRange ManaRange
		{
			get => _manaRange;

			set
			{
				if (value == _manaRange)
				{
					return;
				}

				_manaRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("mvrange")]
		public ValueRange MovesRange
		{
			get => _movesRange;

			set
			{
				if (value == _movesRange)
				{
					return;
				}

				_movesRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange ArmorRange
		{
			get => _armorRange;

			set
			{
				if (value == _armorRange)
				{
					return;
				}

				_armorRange = value;
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

		[OLCAlias("penrange")]
		public ValueRange PenetrationRange
		{
			get => _penetrationRange;

			set
			{
				if (value == _penetrationRange)
				{
					return;
				}

				_penetrationRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("mindamrange")]
		public ValueRange MinimumDamageRange
		{
			get => _minimumDamageRange;

			set
			{
				if (value == _minimumDamageRange)
				{
					return;
				}

				_minimumDamageRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("maxdamrange")]
		public ValueRange MaximumDamageRange
		{
			get => _maximumDamageRange;

			set
			{
				if (value == _maximumDamageRange)
				{
					return;
				}

				_maximumDamageRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public AttackInfo[] Attacks
		{
			get => _attacks;

			set
			{
				if (value == _attacks)
				{
					return;
				}

				_attacks = value;

				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange GoldRange
		{
			get => _goldRange;

			set
			{
				if (value == _goldRange)
				{
					return;
				}

				_goldRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public MobileLoot[] Loot
		{
			get => _loot;

			set
			{
				_loot = value;
			}
		}

		public void OnSerializationStarted()
		{
		}

		public void OnSerializationEnded()
		{
		}

		public CreatureStats CreateStats(int level)
		{
			var stats = new CreatureStats
			{
				MaxHitpoints = HitpointsRange.CalculateValue(level),
				MaxMana = ManaRange.CalculateValue(level),
				MaxMoves = MovesRange.CalculateValue(level),
				Armor = ArmorRange.CalculateValue(level),
			};

			// Calculate attacks' values
			var penetration = PenetrationRange.CalculateValue(level);
			var minimumDamage = MinimumDamageRange.CalculateValue(level);
			var maximumDamage = MaximumDamageRange.CalculateValue(level);

			// Add attacks
			if (Attacks != null)
			{
				foreach (var attackInfo in Attacks)
				{
					if (attackInfo.MinimumLevel > level)
					{
						continue;
					}

					var thisAttackType = attackInfo.IsDefaultAttack ? attackInfo.AttackType : AttackType;
					var thisPenetration = attackInfo.IsDefaultAttack ? penetration : attackInfo.PenetrationRange.CalculateValue(level);
					var thisMinimumDamage = attackInfo.IsDefaultAttack ? minimumDamage : attackInfo.MinimumDamageRange.CalculateValue(level);
					var thisMaximumDamage = attackInfo.IsDefaultAttack ? maximumDamage : attackInfo.MaximumDamageRange.CalculateValue(level);

					var attack = new Attack(thisAttackType, thisPenetration, thisMinimumDamage, thisMaximumDamage);
					stats.Attacks.Add(attack);
				}
			}
			else
			{
				// Add default single attack
				stats.Attacks.Add(new Attack(AttackType, penetration, minimumDamage, maximumDamage));
			}

			long xpAward = Math.Max(1, stats.MaxHitpoints);
			xpAward *= Math.Max(1, stats.Armor);

			long attackXpFactor = 0;
			foreach (var attack in stats.Attacks)
			{
				long t = Math.Max(1, attack.Penetration);

				t *= Math.Max(1, attack.AverageDamage);

				attackXpFactor += t;
			}

			attackXpFactor = Math.Max(1, attackXpFactor / 1000);
			xpAward *= attackXpFactor;

			stats.XpAward = xpAward;

			return stats;
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

				if (asMobile.Class.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public MobileClass CloneClass()
		{
			var clone = new MobileClass
			{
				Id = Id,
				Name = Name,
				Description = Description,
				_hitpointsRange = _hitpointsRange,
				_manaRange = _manaRange,
				_movesRange = _movesRange,
				_armorRange = _armorRange,
				_penetrationRange = _penetrationRange,
				_minimumDamageRange = _minimumDamageRange,
				_maximumDamageRange = _maximumDamageRange,
				_attacks = _attacks,
				_goldRange = _goldRange,
			};

			return clone;
		}

		public object Clone() => CloneClass();

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static MobileClass GetClassById(string name) => Storage.GetByKey(name);
		public static MobileClass EnsureClassById(string name) => Storage.EnsureByKey(name);
	}
}