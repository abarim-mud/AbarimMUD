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
		public ValueRange? PenetrationRange { get; set; }
		public ValueRange? MinimumDamageRange { get; set; }
		public ValueRange? MaximumDamageRange { get; set; }

		public AttackInfo()
		{
		}

		public AttackInfo(int minimumLevel, AttackType attackType, ValueRange? penetration,
			ValueRange minimumDamageRange, ValueRange maximumDamageRange)
		{
			MinimumLevel = minimumLevel;
			AttackType = attackType;
			PenetrationRange = penetration;
			MinimumDamageRange = minimumDamageRange;
			MaximumDamageRange = maximumDamageRange;
		}
	}

	public class GameClass : IStoredInFile, ISerializationEvents, ICloneable
	{
		public const AttackType DefaultAttackType = Data.AttackType.Hit;
		public const int DefaultPenetration = 0;
		public const int DefaultMinimumDamage = 1;
		public const int DefaultMaximumDamage = 4;

		public static readonly ValueRange DefaultHitpoints = new ValueRange(0, 100);
		public static readonly ValueRange DefaultArmor = new ValueRange(0, 0);
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		private ValueRange? _hitpointsRange;
		private ValueRange? _armorRange;
		private AttackType? _attackType;
		private ValueRange? _penetrationRange;
		private ValueRange? _minimumDamageRange;
		private ValueRange? _maximumDamageRange;
		private AttackInfo[] _attacks;

		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsPlayerClass { get; set; }

		/// <summary>
		/// Determines whether inherited properties such as HitPointsRage
		/// Should return their original values
		/// Automatically set to true during the serialization
		/// </summary>
		[JsonIgnore]
		public bool UseOriginalValues { get; set; }

		[JsonConverter(typeof(Common.GameClassConverter))]
		public GameClass Inherits { get; set; }

		public ValueRange? HitpointsRange
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
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public ValueRange? ArmorRange
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
				Creature.InvalidateAllCreaturesStats();
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
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public ValueRange? PenetrationRange
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
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public ValueRange? MinimumDamageRange
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
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public ValueRange? MaximumDamageRange
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
				Creature.InvalidateAllCreaturesStats();
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

				Creature.InvalidateAllCreaturesStats();
			}
		}

		public Dictionary<int, Skill[]> SkillsByLevels { get; set; } = new Dictionary<int, Skill[]>();

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static GameClass GetClassById(string name) => Storage.GetByKey(name);
		public static GameClass EnsureClassById(string name) => Storage.EnsureByKey(name);

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
			var armor = ArmorRange ?? DefaultArmor;

			var stats = new CreatureStats
			{
				MaxHitpoints = hitpoints.CalculateValue(level),
				Armor = armor.CalculateValue(level),
			};

			// Calculate attacks' values
			var attackType = AttackType ?? DefaultAttackType;
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

			return stats;
		}

		public GameClass CloneClass()
		{
			var clone = new GameClass
			{
				Name = Name,
				Description = Description,
				IsPlayerClass = IsPlayerClass,
				Inherits = Inherits,
				_hitpointsRange = _hitpointsRange,
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
	}
}