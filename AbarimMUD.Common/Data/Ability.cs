using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum AbilityType
	{
		Physical,
		Spell
	}

	public enum AbilityFlags
	{
		None,
		Offensive,
		FightSkill,
		TargetFullHp,
		NotFighting
	}

	public class Ability : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Ability> Storage = new Abilities();

		private static Ability _kick;
		private static Ability _backstab;
		private static Ability _circlestab;
		private static Ability _strike;
		private static Ability _deathtouch;

		public static Ability Kick
		{
			get
			{
				if (_kick == null)
				{
					_kick = EnsureAbilityById("kick");
				}

				return _kick;
			}
		}

		public static Ability Backstab
		{
			get
			{
				if (_backstab == null)
				{
					_backstab = EnsureAbilityById("backstab");
				}

				return _backstab;
			}
		}

		public static Ability Circlestab
		{
			get
			{
				if (_circlestab == null)
				{
					_circlestab = EnsureAbilityById("circlestab");
				}

				return _circlestab;
			}
		}

		public static Ability Strike
		{
			get
			{
				if (_strike == null)
				{
					_strike = EnsureAbilityById("strike");
				}

				return _strike;
			}
		}


		public static Ability Deathtouch
		{
			get
			{
				if (_deathtouch == null)
				{
					_deathtouch = EnsureAbilityById("deathtouch");
				}

				return _deathtouch;
			}
		}

		[OLCIgnore]
		public string Id { get; set; }
		public string Name { get; set; }
		public AbilityType Type { get; set; }
		public PlayerClass PrimeClass { get; set; }

		public int MovesCost { get; set; }
		public int ManaCost { get; set; }
		public string MessageMissUser { get; set; }
		public string MessageMissRoom { get; set; }
		public string MessageHitUser { get; set; }
		public string MessageHitTarget { get; set; }
		public string MessageHitRoom { get; set; }
		public string MessageKillUser { get; set; }
		public string MessageKillRoom { get; set; }
		public string MessageDeactivatedUser { get; set; }
		public string Description { get; set; }

		public Dictionary<ModifierType, Affect> Affects { get; set; }
		public InstantEffect[] InstantEffects { get; set; }

		public HashSet<AbilityFlags> Flags { get; set; } = new HashSet<AbilityFlags>();
		public int? CommandLagInMs { get; set; }

		[JsonIgnore]
		public int PhysicalCommandLagInMs => CommandLagInMs ?? Configuration.PauseBetweenFightRoundsInMs;


		public override string ToString()
		{
			if (Type == AbilityType.Physical)
			{
				return Name;
			}

			return $"cast '{Name}'";
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Ability GetAbilityById(string name) => Storage.GetByKey(name);
		public static Ability EnsureAbilityById(string name) => Storage.EnsureByKey(name);
	}
}
