using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.Utils;
using System;

namespace AbarimMUD.Data
{
	public class PlayerClass : IStoredInFile, ICloneable
	{
		public static readonly MultipleFilesStorage<PlayerClass> Storage = new PlayerClasses();

		private ValueRange _hitpointsRange, _manaRange, _movesRange;

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

		public Item[] StartingEquipment { get; set; }
		public Skill[] StartingSkills { get; set; }
		public Inventory StartingInventory { get; set; }
		public int StartingGold { get; set; }

		public CreatureStats CreateStats(int level)
		{
			var hitpoints = HitpointsRange;
			var mana = ManaRange;
			var moves = MovesRange;

			var stats = new CreatureStats
			{
				MaxHitpoints = hitpoints.CalculateValue(level),
				MaxMana = mana.CalculateValue(level),
				MaxMoves = moves.CalculateValue(level),
			};

			return stats;
		}

		private void InvalidateCreaturesOfThisClass()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asCharacter = creature as Character;
				if (asCharacter == null)
				{
					continue;
				}

				if (asCharacter.Class.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public PlayerClass CloneClass()
		{
			var clone = new PlayerClass
			{
				Id = Id,
				Name = Name,
				Description = Description,
				_hitpointsRange = _hitpointsRange,
				_manaRange = _manaRange,
				_movesRange = _movesRange,
			};

			return clone;
		}

		public object Clone() => CloneClass();

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static PlayerClass GetClassById(string name) => Storage.GetByKey(name);
		public static PlayerClass EnsureClassById(string name) => Storage.EnsureByKey(name);
	}
}