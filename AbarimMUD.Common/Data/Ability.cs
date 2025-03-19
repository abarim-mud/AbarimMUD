using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public enum AbilityType
	{
		Passive,
		Physical,
		Custom
	}

	public class Ability : IStoredInFile
	{
		public static readonly MultipleFilesStorage<Ability> Storage = new Abilities();

		private static Ability _kick;
		private static Ability _backstab;
		private static Ability _circlestab;
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
		public Dictionary<ModifierType, int> Modifiers { get; set; }
		public PlayerClass PrimeClass { get; set; }
		public int MovesCost { get; set; }

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static Ability GetAbilityById(string name) => Storage.GetByKey(name);
		public static Ability EnsureAbilityById(string name) => Storage.EnsureByKey(name);
	}
}
