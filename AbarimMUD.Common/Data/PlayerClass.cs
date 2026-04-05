using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class PlayerClass : IStoredInFile
	{
		public static readonly MultipleFilesStorage<PlayerClass> Storage = new PlayerClasses();
		private int _startHitpoints, _startMana, _startMoves;
		private int _hitpointsPerLevel, _manaPerLevel, _movesPerLevel;

		[OLCIgnore]
		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }

		public int StartHitpoints
		{
			get => _startHitpoints;

			set
			{
				if (value == _startHitpoints)
				{
					return;
				}

				_startHitpoints = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int StartMana
		{
			get => _startMana;

			set
			{
				if (value == _startMana)
				{
					return;
				}

				_startMana = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int StartMoves
		{
			get => _startMoves;

			set
			{
				if (value == _startMoves)
				{
					return;
				}

				_startMoves = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int HitpointsPerLevel
		{
			get => _hitpointsPerLevel;

			set
			{
				if (value == _hitpointsPerLevel)
				{
					return;
				}

				_hitpointsPerLevel = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int ManaPerLevel
		{
			get => _manaPerLevel;

			set
			{
				if (value == _manaPerLevel)
				{
					return;
				}

				_manaPerLevel = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public int MovesPerLevel
		{
			get => _movesPerLevel;

			set
			{
				if (value == _movesPerLevel)
				{
					return;
				}

				_movesPerLevel = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public List<ItemInstance> StartingEquipment { get; set; } = new List<ItemInstance>();
		public List<Skill> StartingSkills { get; set; } = new List<Skill>();
		public Inventory StartingInventory { get; set; }
		public int StartingGold { get; set; }
		public int StartingSkillPoints { get; set; }

		public CreatureStats CreateStats(int level)
		{
			// Make the level zero-based
			--level;
			
			var stats = new CreatureStats
			{
				HitpointsBase = StartHitpoints + level * HitpointsPerLevel,
				ManaBase = StartMana + level * ManaPerLevel,
				MovesBase = StartMoves + level * MovesPerLevel,
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

		public override string ToString() => Name;

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static PlayerClass GetClassById(string name) => Storage.GetByKey(name);
		public static PlayerClass EnsureClassById(string name) => Storage.EnsureByKey(name);
	}
}