using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum Role
	{
		Player,
		Builder,
		Administrator,
		Owner
	}

	public enum FightDetails
	{
		None,
		Damage,
		All
	}

	public sealed class Character : Creature, IStoredInFile
	{
		public static readonly List<Character> ActiveCharacters = new List<Character>();

		public static readonly MultipleFilesStorage<Character> Storage = new Characters();

		private PlayerClass _class;
		private int _level, _skillPoints;
		private Room _room;


		[JsonIgnore]
		public Account Account { get; set; }

		[OLCIgnore]
		public DateTime LastLogin { get; set; }

		public Role Role { get; set; }

		[JsonIgnore]
		public bool IsStaff
		{
			get { return Role >= Role.Builder; }
		}

		[JsonIgnore]
		public string Id
		{
			get => Name;
			set => Name = value;
		}

		[OLCIgnore]
		public string Name { get; set; }


		[OLCAlias("description")]
		public string PlayerDescription { get; set; }

		[OLCAlias("class")]

		public PlayerClass Class
		{
			get => _class;

			set
			{
				_class = value;
				InvalidateStats();
			}
		}

		public string ClassName => Class.Name;

		[OLCAlias("level")]
		public int PlayerLevel
		{
			get => _level;
			set
			{
				if (value < 1)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_level = value;
				InvalidateStats();
			}
		}

		public int SkillPoints
		{
			get => _skillPoints;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				_skillPoints = value;
			}
		}

		[OLCAlias("sex")]
		public Sex PlayerSex { get; set; }

		public long Experience { get; set; }

		public SortedDictionary<string, SkillValue> Skills { get; set; } = new SortedDictionary<string, SkillValue>();

		public int SpentSkillPointsCount
		{
			get
			{
				var result = 0;
				foreach (var pair in Skills)
				{
					result += pair.Value.Level;
				}

				result -= Class.StartingSkills.Count;

				return result;
			}
		}

		public string FightSkill { get; set; }
		public string StabWeapon { get; set; }
		public string Title { get; set; }
		public FightDetails FightDetails { get; set; } = FightDetails.Damage;

		public override string ShortDescription => Name;
		public override string Description => PlayerDescription;
		public override int Level => PlayerLevel;
		public override Sex Sex => PlayerSex;

		[JsonIgnore]
		[OLCIgnore]
		public override Room Room
		{
			get { return _room; }

			set
			{
				if (_room != null)
				{
					_room.Characters.Remove(this);
				}

				_room = value;

				if (_room != null)
				{
					_room.Characters.Add(this);
				}

				RoomChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public override HashSet<string> Keywords => new HashSet<string> { Name };

		public override event EventHandler RoomChanged;

		public Character()
		{
		}

		private void UpdateLevel()
		{
			if (_level == 0)
			{
				// Level not set
				return;
			}

			var leveled = false;
			while (_level < Configuration.MaximumLevel)
			{
				var levelInfo = LevelInfo.GetLevelInfo(_level + 1);
				if (Experience < levelInfo.Experience)
				{
					break;
				}

				leveled = true;
				Experience -= levelInfo.Experience;
				++_level;

				++SkillPoints;
				if (_level % 10 == 0)
				{
					++SkillPoints;
				}
			}

			if (leveled)
			{
				InvalidateStats();
			}
		}

		public override string ToString() => $"{Name}, {Role}, {Class.Name}, {Level}";

		public void Create() => Storage.Create(this);
		public void Save()
		{
			if (Account != null)
			{
				Storage.Save(this);
			}
		}

		public string BuildCharacterFolder()
		{
			var result = Account.BuildAccountFolder();

			// Add character name in the path
			result = Path.Combine(result, Name);

			return result;
		}

		public void GainXp(long experience)
		{
			Experience += experience;

			// Process level ups
			UpdateLevel();

			Save();
		}

		protected override void EnumerateModifiers(ModifiersAccumulator result)
		{
			base.EnumerateModifiers(result);

			// Apply skills and abilities
			foreach (var pair in Skills)
			{
				var skill = pair.Value.Skill;

				// Apply all levels up to learned one
				for (var level = 0; level < pair.Value.Level; ++level)
				{
					var def = skill.Levels[level];

					foreach (var modPair in def.Modifiers)
					{
						result.Add(modPair.Key, modPair.Value);
					}

					if (Class.Id.EqualsToIgnoreCase(skill.Class.Id))
					{
						// Prime class skill
						// Apply prime modifiers too
						foreach (var modPair in def.PrimeModifiers)
						{
							result.Add(modPair.Key, modPair.Value);
						}
					}

					if (def.Abilities != null)
					{
						foreach (var ab in def.Abilities)
						{
							if (ab.PrimeClass != null && ab.PrimeClass.Id != Class.Id)
							{
								// Prime ability
								continue;
							}

							if (ab.Modifiers != null)
							{
								foreach (var modPair in ab.Modifiers)
								{
									result.Add(modPair.Key, modPair.Value);
								}
							}

							if (ab.Type != AbilityType.Passive)
							{
								result.Abilities.Add(ab);
							}
						}
					}
				}
			}
		}

		protected override CreatureStats CreateBaseStats(int attacksCount)
		{
			var result = Class.CreateStats(Level);

			// Default attack
			var attack = new Attack(AttackType.Hit, 0, Configuration.CharacterBarehandedDamage.Minimum, Configuration.CharacterBarehandedDamage.Maximum);

			// Set attacks
			for (var i = 0; i < attacksCount; ++i)
			{
				result.Attacks.Add(attack.Clone());
			}

			return result;
		}

		public override bool MatchesKeyword(string keyword) => Name.StartsWith(keyword, StringComparison.OrdinalIgnoreCase);

		public void Train(Skill skill)
		{
			SkillValue skillValue = null;
			foreach (var pair in Skills)
			{
				if (skill != null && skill.Id == pair.Value.Skill.Id)
				{
					skillValue = pair.Value;
				}
			}

			if (skillValue != null && skillValue.IsMaxed)
			{
				return;
			}

			if (skillValue == null)
			{
				Skills[skill.Id] = new SkillValue(skill);
			}
			else
			{
				++skillValue.Level;
			}

			InvalidateStats();
		}

		public static Character GetCharacterByName(string name) => Storage.GetByKey(name);
		public static Character EnsureCharacterByName(string name) => Storage.EnsureByKey(name);
		public static Character LookupCharacter(string name) => Storage.Lookup(name);
		public static Character[] GetCharactersByAccountName(string name)
		{
			var result = (from c in Storage where c.Account.Name.ToLower() == name.ToLower() select c).ToArray();

			return result;
		}
	}
}