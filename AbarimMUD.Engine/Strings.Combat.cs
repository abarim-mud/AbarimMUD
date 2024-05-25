using AbarimMUD.Data;
using System;
using System.Text;

namespace AbarimMUD
{
	static partial class Strings
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

		static Strings()
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
		}

		public static string GetAttackNoun(this AttackType attackType)
		{
			return attackType.ToString().ToLower();
		}

		public static string GetAttackVerb(AttackType attackType)
		{
			return _attackNames[(int)attackType].Verb;
		}

		public static string GetEvadeMessage(string attackerName, string targetName, AttackType attackType)
		{
			return $"{targetName} evades {GetAttackNoun(attackType)} of {attackerName}.";
		}

		public static string GetArmorMessage(string attackerName, string targetName, AttackType attackType, bool blocked)
		{
			var sb = new StringBuilder();
			if (blocked)
			{
				sb.Append("Blocked. ");
			}

			sb.Append($"{attackerName} couldn't pierce through the armor of {targetName} with {GetAttackNoun(attackType)}.");

			return sb.ToString();
		}

		public static string GetAttackMessage(int damage, string attackerName, string targetName, AttackType attackType)
		{
			var sb = new StringBuilder();
			if (damage < 5)
			{
				sb.Append($"{attackerName} barely {GetAttackVerb(attackType)} {targetName} ({damage}).");
			}
			else if (damage < 10)
			{
				sb.Append($"{attackerName} {GetAttackVerb(attackType)} {targetName} ({damage}).");
			}
			else if (damage < 15)
			{
				sb.Append($"{attackerName} {GetAttackVerb(attackType)} {targetName} hard ({damage}).");
			}
			else if (damage < 20)
			{
				sb.Append($"{attackerName} {GetAttackVerb(attackType)} {targetName} very hard ({damage}).");
			}
			else if (damage < 25)
			{
				sb.Append($"{attackerName} {GetAttackVerb(attackType)} {targetName} extremelly hard ({damage}).");
			}
			else if (damage < 30)
			{
				sb.Append($"{attackerName} massacres {targetName} to small fragments with {GetAttackNoun(attackType)} ({damage}).");
			}
			else if (damage < 50)
			{
				sb.Append($"{attackerName} brutally massacres {targetName} to small fragments with {GetAttackNoun(attackType)} ({damage}).");
			}
			else
			{
				sb.Append($"{attackerName} viciously massacres {targetName} to small fragments with {GetAttackNoun(attackType)} ({damage}).");
			}

			return sb.ToString();
		}

		public static string GetNpcDeathMessage(string name)
		{
			return string.Format("{0} is dead! R.I.P. Your blood freezes as you hear {0}'s death cry.", name);
		}

		public static string GetExpMessage(int experience)
		{
			return string.Format("You had been awarded {0} experience.", experience);
		}
	}
}
