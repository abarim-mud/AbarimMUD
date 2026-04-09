using AbarimMUD.Utils;
using System;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum AttackType
	{
		Hit,
		Slice,
		Stab,
		Slash,
		Whip,
		Claw,
		Hack,
		Blast,
		Pound,
		Crush,
		Grep,
		Bite,
		Pierce,
		Suction,
		Beating,
		Charge,
		Slap,
		Punch,
		Cleave,
		Scratch,
		Peck,
		Chop,
		Sting,
		Smash,
		Chomp,
		Thrust,
		Slime,
		Shock,
		Bludgeon,
		Rake,
		Beat,
		Zap,
		Gore
	}

	public struct AttackRollResult
	{
		public bool Hit;
		public int AttackDice;
		public int AttackRoll;

		public override string ToString()
		{
			if (AttackDice <= 10)
			{
				return "miss";
			}

			if (AttackDice >= 190)
			{
				return "hit";
			}

			return $"{AttackRoll}";
		}
	}

	public class Attack
	{
		public AttackType Type { get; set; }

		public int AttackBonus { get; set; }

		public ValueRange DamageRange;

		[JsonIgnore]
		public int MinimumDamage
		{
			get => DamageRange.Minimum;

			set => DamageRange.Minimum = value;
		}

		[JsonIgnore]
		public int MaximumDamage
		{
			get => DamageRange.Maximum;

			set => DamageRange.Maximum = value;
		}

		public int AverageDamage => MinimumDamage + (MaximumDamage - MinimumDamage) / 2;

		public Attack()
		{
		}

		public Attack(AttackType type, int attackBonus, ValueRange damageRange)
		{
			Type = type;
			AttackBonus = attackBonus;
			DamageRange = damageRange;
		}

		public Attack(AttackType attackType, int attackBonus, int min, int max) : this(attackType, attackBonus, new ValueRange(min, max))
		{
		}

		public Attack Clone() => new Attack(Type, AttackBonus, DamageRange);

		public AttackRollResult DoAttackRoll(int armorClass)
		{
			var result = new AttackRollResult
			{
				AttackDice = Utility.RandomRange(1, 200)
			};

			if (result.AttackDice <= 10)
			{
				// Always miss
			}
			else if (result.AttackDice >= 190)
			{
				// Always hit
				result.Hit = true;
			}
			else
			{
				//
				result.AttackRoll = result.AttackDice + AttackBonus - 100;
				result.Hit = result.AttackRoll >= armorClass;
			}

			return result;
		}

		public int CalculateDamage(int damageReduction = 0)
		{
			var result = DamageRange.Random();

			var reduction = result * damageReduction / 100.0f;

			result -= (int)reduction;

			if (result < 1)
			{
				result = 1;
			}

			return result;
		}

		public override string ToString()
		{
			return $"{Type}, {AttackBonus}, {DamageRange}";
		}
	}

	public static class AttackExtensions
	{
		private class AttackNames
		{
			public string Verb;

			public AttackNames(string verb)
			{
				Verb = verb;
			}
		}

		private static readonly AttackNames[] _attackNames = new AttackNames[Enum.GetNames(typeof(AttackType)).Length];

		static AttackExtensions()
		{
			_attackNames[(int)AttackType.Hit] = new AttackNames("hits");
			_attackNames[(int)AttackType.Slice] = new AttackNames("slices");
			_attackNames[(int)AttackType.Stab] = new AttackNames("stabs");
			_attackNames[(int)AttackType.Slash] = new AttackNames("slashes");
			_attackNames[(int)AttackType.Whip] = new AttackNames("whips");
			_attackNames[(int)AttackType.Claw] = new AttackNames("claws");
			_attackNames[(int)AttackType.Hack] = new AttackNames("hacks");
			_attackNames[(int)AttackType.Blast] = new AttackNames("blasts");
			_attackNames[(int)AttackType.Pound] = new AttackNames("pounds");
			_attackNames[(int)AttackType.Crush] = new AttackNames("crushes");
			_attackNames[(int)AttackType.Grep] = new AttackNames("greps");
			_attackNames[(int)AttackType.Bite] = new AttackNames("bites");
			_attackNames[(int)AttackType.Pierce] = new AttackNames("pierces");
			_attackNames[(int)AttackType.Suction] = new AttackNames("suctions");
			_attackNames[(int)AttackType.Beating] = new AttackNames("beats");
			_attackNames[(int)AttackType.Charge] = new AttackNames("charges");
			_attackNames[(int)AttackType.Slap] = new AttackNames("slaps");
			_attackNames[(int)AttackType.Punch] = new AttackNames("punches");
			_attackNames[(int)AttackType.Cleave] = new AttackNames("cleaves");
			_attackNames[(int)AttackType.Scratch] = new AttackNames("scratches");
			_attackNames[(int)AttackType.Peck] = new AttackNames("pecks");
			_attackNames[(int)AttackType.Chop] = new AttackNames("chops");
			_attackNames[(int)AttackType.Sting] = new AttackNames("stings");
			_attackNames[(int)AttackType.Smash] = new AttackNames("smashes");
			_attackNames[(int)AttackType.Chomp] = new AttackNames("chomps");
			_attackNames[(int)AttackType.Thrust] = new AttackNames("thrusts");
			_attackNames[(int)AttackType.Slime] = new AttackNames("slimes");
			_attackNames[(int)AttackType.Shock] = new AttackNames("shocks");
			_attackNames[(int)AttackType.Bludgeon] = new AttackNames("bludgeons");
			_attackNames[(int)AttackType.Rake] = new AttackNames("rakes");
			_attackNames[(int)AttackType.Beat] = new AttackNames("beats");
			_attackNames[(int)AttackType.Zap] = new AttackNames("zaps");
			_attackNames[(int)AttackType.Gore] = new AttackNames("gores");
		}

		public static string GetAttackNoun(this AttackType attackType)
		{
			return attackType.ToString().ToLower();
		}

		public static string GetAttackVerb(this AttackType attackType)
		{
			return _attackNames[(int)attackType].Verb;
		}
	}
}
