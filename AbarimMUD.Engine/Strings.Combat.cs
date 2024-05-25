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

		public static string GetAttackVerb(this AttackType attackType)
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

		public static string GetAttackMessage(DamageResult damageResult, string attackerName, string targetName, AttackType attackType)
		{
			string result;
			string attackVerb, massacre, massacre2;

			if (attackerName == "You")
			{
				attackVerb = attackType.GetAttackNoun();
				massacre = "massacre";
				massacre2 = $"your {attackType.GetAttackNoun()}";
			} else
			{
				attackVerb = attackType.GetAttackVerb();
				massacre = "massacres";
				massacre2 = $"its {attackType.GetAttackNoun()}";
			}

			var damage = damageResult.Damage;
			if (damage < 5)
			{
				result = $"{attackerName} barely {attackVerb} {targetName} ({damageResult}).";
			}
			else if (damage < 10)
			{
				result = $"{attackerName} {attackVerb} {targetName} ({damageResult}).";
			}
			else if (damage < 15)
			{
				result = $"{attackerName} {attackVerb} {targetName} hard ({damageResult}).";
			}
			else if (damage < 20)
			{
				result = $"{attackerName} {attackVerb} {targetName} very hard ({damageResult}).";
			}
			else if (damage < 25)
			{
				result = $"{attackerName} {attackVerb} {targetName} extremelly hard ({damageResult}).";
			}
			else if (damage < 30)
			{
				result = $"{attackerName} {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}
			else if (damage < 50)
			{
				result = $"{attackerName} brutally {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}
			else
			{
				result = $"{attackerName} viciously {massacre} {targetName} to small fragments with {massacre2} ({damageResult}).";
			}

			return result;
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
