using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.Utils;
using System.Collections.Generic;
using Ur;

namespace AbarimMUD.Data
{
	public class PlayerClass : IStoredInFile
	{
		public static readonly MultipleFilesStorage<PlayerClass> Storage = new PlayerClasses();
		private ValueRange _hitpoints, _mana, _moves;

		[OLCIgnore]
		public string Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }

		public ValueRange Hitpoints
		{
			get => _hitpoints;

			set
			{
				if (value == _hitpoints)
				{
					return;
				}

				_hitpoints = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange Mana
		{
			get => _mana;

			set
			{
				if (value == _mana)
				{
					return;
				}

				_mana = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public ValueRange Moves
		{
			get => _moves;

			set
			{
				if (value == _moves)
				{
					return;
				}

				_moves = value;
				InvalidateCreaturesOfThisClass();
			}
		}

		public List<ItemInstance> StartingEquipment { get; set; } = new List<ItemInstance>();
		public List<Skill> StartingSkills { get; set; } = new List<Skill>();
		public Inventory StartingInventory { get; set; }
		public int StartingGold { get; set; }
		public int StartingSkillPoints { get; set; }
		public Dictionary<ModifierType, int> PrimeModifiers { get; set; } = new Dictionary<ModifierType, int>();
		public Dictionary<string, AbilityPower> PrimeAbilities { get; set; } = new Dictionary<string, AbilityPower>();
		
		public CreatureStats CreateStats(int level)
		{
			return new CreatureStats
			{
				HitpointsBase = Hitpoints.GetValueByLevel(level),
				ManaBase = Mana.GetValueByLevel(level),
				MovesBase = Moves.GetValueByLevel(level),
			};
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