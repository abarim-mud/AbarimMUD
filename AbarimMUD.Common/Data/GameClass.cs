using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.StorageAPI;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class AttackInfo
	{
		public int MinimumLevel { get; set; }
		public AttackType? AttackType { get; set; }
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
			AttackType = attackType;
			PenetrationRange = penetration;
			MinimumDamageRange = minimumDamageRange;
			MaximumDamageRange = maximumDamageRange;
		}
	}

	[Flags]
	public enum GameClassFlags
	{
		None = 0,

		/// <summary>
		/// Could be used only as base class
		/// </summary>
		Abstract = 1 << 0,

		/// <summary>
		/// Could be used by a player
		/// </summary>
		Player = 1 << 1,

		/// <summary>
		/// Could be used by a mobile
		/// </summary>
		Mobile = 1 << 2,
	}

	public class EqSet
	{
		public int MinimumLevel { get; set; }
		public Item[] Items { get; set; }
	}

	public class GameClass : IStoredInFile, ISerializationEvents, ICloneable
	{
		public const AttackType DefaultAttackType = Data.AttackType.Hit;
		public const int DefaultPenetration = 0;
		public const int DefaultMinimumDamage = 1;
		public const int DefaultMaximumDamage = 4;

		public static readonly ValueRange DefaultHitpoints = new ValueRange(1, 100);
		public static readonly ValueRange DefaultHitpointsRegen = new ValueRange(1, 50);
		public static readonly ValueRange DefaultMana = new ValueRange(100, 200);
		public static readonly ValueRange DefaultManaRegen = new ValueRange(1, 50);
		public static readonly ValueRange DefaultMoves = new ValueRange(100, 200);
		public static readonly ValueRange DefaultMovesRegen = new ValueRange(1, 50);
		public static readonly ValueRange DefaultArmor = new ValueRange(0, 0);
		public static readonly MultipleFilesStorage<GameClass> Storage = new GameClasses();

		private AttackType? _attackType;
		private ValueRange _hitpointsRange, _manaRange = DefaultMana, _movesRange;
		private ValueRange _hitpointsRegenRange, _manaRegenRange = DefaultManaRegen, _movesRegenRange;
		private ValueRange _armorRange;
		private ValueRange _penetrationRange;
		private ValueRange _minimumDamageRange;
		private ValueRange _maximumDamageRange;
		private AttackInfo[] _attacks;
		private EqSet[] _eqSets;

		[OLCIgnore]
		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public GameClassFlags Flags { get; set; }

		/// <summary>
		/// Determines whether inherited properties such as HitPointsRage
		/// Should return their original values
		/// Automatically set to true during the serialization
		/// </summary>
		[JsonIgnore]
		public bool UseOriginalValues { get; set; }

		[JsonConverter(typeof(Common.GameClassConverter))]
		public GameClass Inherits { get; set; }

		[OLCAlias("hprange")]
		public ValueRange HitpointsRange
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _hitpointsRange != null)
				{
					return _hitpointsRange;
				}

				return Inherits.HitpointsRange;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _manaRange != null)
				{
					return _manaRange;
				}

				return Inherits.ManaRange;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _movesRange != null)
				{
					return _movesRange;
				}

				return Inherits.MovesRange;
			}

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

		[OLCAlias("hpregenrange")]
		public ValueRange HitpointsRegenRange
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _hitpointsRegenRange != null)
				{
					return _hitpointsRegenRange;
				}

				return Inherits.HitpointsRegenRange;
			}

			set
			{
				if (value == _hitpointsRegenRange)
				{
					return;
				}

				_hitpointsRegenRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("manaregenrange")]
		public ValueRange ManaRegenRange
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _manaRegenRange != null)
				{
					return _manaRegenRange;
				}

				return Inherits.ManaRegenRange;
			}

			set
			{
				if (value == _manaRegenRange)
				{
					return;
				}

				_manaRegenRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		[OLCAlias("mvregenrange")]
		public ValueRange MovesRegenRange
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _movesRegenRange != null)
				{
					return _movesRegenRange;
				}

				return Inherits.MovesRegenRange;
			}

			set
			{
				if (value == _movesRegenRange)
				{
					return;
				}

				_movesRegenRange = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange ArmorRange
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _armorRange != null)
				{
					return _armorRange;
				}

				return Inherits.ArmorRange;
			}

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

		public AttackType? AttackType
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _attackType != null)
				{
					return _attackType;
				}

				return Inherits.AttackType;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _penetrationRange != null)
				{
					return _penetrationRange;
				}

				return Inherits.PenetrationRange;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _minimumDamageRange != null)
				{
					return _minimumDamageRange;
				}

				return Inherits.MinimumDamageRange;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _maximumDamageRange != null)
				{
					return _maximumDamageRange;
				}

				return Inherits.MaximumDamageRange;
			}

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
			get
			{
				if (UseOriginalValues || Inherits == null || _attacks != null)
				{
					return _attacks;
				}

				return Inherits.Attacks;
			}

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

		public EqSet[] EqSets
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _eqSets != null)
				{
					return _eqSets;
				}

				return Inherits.EqSets;
			}

			set
			{
				_eqSets = value;
			}
		}

		public SortedDictionary<int, Skill[]> SkillsByLevels { get; set; } = new SortedDictionary<int, Skill[]>();

		public void OnSerializationStarted()
		{
			UseOriginalValues = true;
		}

		public void OnSerializationEnded()
		{
			UseOriginalValues = false;
		}

		public CreatureStats CreateStats(int level)
		{
			var hitpoints = HitpointsRange ?? DefaultHitpoints;
			var hitpointsRegen = HitpointsRegenRange ?? DefaultHitpointsRegen;
			var mana = ManaRange ?? DefaultMana;
			var manaRegen = ManaRegenRange ?? DefaultManaRegen;
			var moves = MovesRange ?? DefaultMoves;
			var movesRegen = MovesRegenRange ?? DefaultMovesRegen;
			var armor = ArmorRange ?? DefaultArmor;

			var stats = new CreatureStats
			{
				MaxHitpoints = hitpoints.CalculateValue(level),
				HitpointsRegen = hitpointsRegen.CalculateValue(level),
				MaxMana = mana.CalculateValue(level),
				ManaRegen = manaRegen.CalculateValue(level),
				MaxMoves = moves.CalculateValue(level),
				MovesRegen = movesRegen.CalculateValue(level),
				Armor = armor.CalculateValue(level),
			};

			// Calculate attacks' values
			var attackType = AttackType ?? DefaultAttackType;

			// We use sqrt growth type for players and linear growth type for mobs
			var penetration = PenetrationRange.CalculateValue(level, DefaultPenetration);
			var minimumDamage = MinimumDamageRange.CalculateValue(level, DefaultMinimumDamage);
			var maximumDamage = MaximumDamageRange.CalculateValue(level, DefaultMaximumDamage);

			// Add attacks
			if (Attacks != null)
			{
				foreach (var attackInfo in Attacks)
				{
					if (attackInfo.MinimumLevel > level)
					{
						continue;
					}

					var thisAttackType = attackInfo.AttackType ?? attackType;
					var thisPenetration = penetration;
					if (attackInfo.PenetrationRange != null)
					{
						thisPenetration = attackInfo.PenetrationRange.CalculateValue(level);
					}

					var thisMinimumDamage = minimumDamage;
					if (attackInfo.MinimumDamageRange != null)
					{
						thisMinimumDamage = attackInfo.MinimumDamageRange.CalculateValue(level);
					}

					var thisMaximumDamage = maximumDamage;
					if (attackInfo.MaximumDamageRange != null)
					{
						thisMaximumDamage = attackInfo.MaximumDamageRange.CalculateValue(level);
					}

					var attack = new Attack(thisAttackType, thisPenetration, thisMinimumDamage, thisMaximumDamage);
					stats.Attacks.Add(attack);
				}
			}
			else
			{
				// Add default single attack
				stats.Attacks.Add(new Attack(attackType, penetration, minimumDamage, maximumDamage));
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
			stats.BackstabMultiplier = CombatCalc.BackstabMult(level);

			// Apply skills
			foreach (var pair in SkillsByLevels)
			{
				if (pair.Key > level)
				{
					// Since SkillsByLevels is SortedDictionary, we could break upon reaching the first out-of-level pair
					break;
				}

				foreach (var skill in pair.Value)
				{
					foreach (var mod in skill.Modifiers)
					{
						stats.ApplyModifier(mod.Key, mod.Value);
					}
				}
			}

			return stats;
		}

		private void InvalidateCreaturesOfThisClass()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				if (creature.Class.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public GameClass CloneClass()
		{
			var clone = new GameClass
			{
				Id = Id,
				Name = Name,
				Description = Description,
				Flags = Flags,
				Inherits = Inherits,
				_hitpointsRange = _hitpointsRange,
				_hitpointsRegenRange = _hitpointsRegenRange,
				_manaRange = _manaRange,
				_manaRegenRange = _manaRegenRange,
				_movesRange = _movesRange,
				_movesRegenRange = _movesRegenRange,
				_armorRange = _armorRange,
				_penetrationRange = _penetrationRange,
				_minimumDamageRange = _minimumDamageRange,
				_maximumDamageRange = _maximumDamageRange,
				_attacks = _attacks,
			};

			foreach (var pair in SkillsByLevels)
			{
				clone.SkillsByLevels[pair.Key] = pair.Value;
			}

			return clone;
		}

		public object Clone() => CloneClass();

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static GameClass GetClassById(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassById(string name) => Storage.EnsureByKey(name);
	}
}