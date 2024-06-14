using AbarimMUD.Storage;
using AbarimMUD.StorageAPI;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public struct RaceClassValueRange
	{
		public int Level1Value;
		public int Level100Value;

		public RaceClassValueRange(int level1Value, int level100Value)
		{
			Level1Value = level1Value;
			Level100Value = level100Value;
		}

		public int CalculateValue(int level)
		{
			if (level < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(level));
			}

			if (level == 1)
			{
				return Level1Value;
			}

			if (level == 100)
			{
				return Level100Value;
			}

			// Sqrt interpolation
			var k = (float)Math.Sqrt((level - 1) / 99.0f);
			var value = Level1Value + k * (Level100Value - Level1Value);

			return (int)value;
		}

		public override bool Equals(object obj)
		{
			return obj is RaceClassValueRange && base.Equals((RaceClassValueRange)obj);
		}

		public bool Equals(RaceClassValueRange obj)
		{
			return Level1Value == obj.Level1Value && Level100Value == obj.Level100Value;
		}

		public static bool operator ==(RaceClassValueRange left, RaceClassValueRange right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(RaceClassValueRange left, RaceClassValueRange right)
		{
			return !left.Equals(right);
		}
	}

	public class GameClass : IStoredInFile, ISerializationEvents
	{
		public static readonly MultipleFilesStorageString<GameClass> Storage = new GameClasses();

		private RaceClassValueRange? _hitpoints;
		private RaceClassValueRange? _penetration;
		private RaceClassValueRange? _damageBonus;

		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }

		/// <summary>
		/// Determines whether inherited properties such as HitPointsRage
		/// Should return their original values
		/// Automatically set to true during the serialization
		/// </summary>
		[JsonIgnore]
		public bool UseOriginalValues { get; set; }

		[JsonConverter(typeof(Common.GameClassConverter))]
		public GameClass Inherits { get; set; }

		public RaceClassValueRange? Hitpoints
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _hitpoints != null)
				{
					return _hitpoints;
				}

				return Inherits.Hitpoints;
			}

			set
			{
				if (value == _hitpoints)
				{
					return;
				}

				_hitpoints = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public RaceClassValueRange? Penetration
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _penetration != null)
				{
					return _penetration;
				}

				return Inherits.Penetration;
			}

			set
			{
				if (value == _penetration)
				{
					return;
				}

				_penetration = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public RaceClassValueRange? DamageBonus
		{
			get
			{
				if (UseOriginalValues || Inherits == null || _damageBonus != null)
				{
					return _damageBonus;
				}

				return Inherits.DamageBonus;
			}

			set
			{
				if (value == _damageBonus)
				{
					return;
				}

				_damageBonus = value;
				Creature.InvalidateAllCreaturesStats();
			}
		}

		public Dictionary<int, Dictionary<ModifierType, int>> LevelsBonuses { get; set; } = new Dictionary<int, Dictionary<ModifierType, int>>();

		public bool IsPlayerClass { get; set; }

		public Dictionary<int, List<Skill>> SkillsByLevels { get; set; } = new Dictionary<int, List<Skill>>();

		public override string ToString() => Name;

		public void EnsureHitpointsRangeSet()
		{
			if (Hitpoints == null)
			{
				throw new Exception($"Class {Name} or its parent doesnt have hitpoints rage set.");
			}
		}

		public void EnsurePenetrationSet()
		{
			if (Penetration == null)
			{
				throw new Exception($"Class {Name} or its parent doesnt have penetration set.");
			}
		}

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
	}
}
