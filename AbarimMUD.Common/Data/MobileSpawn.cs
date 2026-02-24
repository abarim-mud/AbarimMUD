using AbarimMUD.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class MobileSpawn
	{
		private Shop _shop;

		[Browsable(false)]
		public Mobile Mobile { get; set; }

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; }

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

		[Browsable(false)]
		public PlayerClass Guildmaster { get; set; }

		public Shop Shop
		{
			get => _shop;

			set
			{
				if (value == _shop)
				{
					return;
				}

				_shop = value;

				// Rebuild inventories
				foreach (var creature in Creature.ActiveCreatures)
				{
					var asMobile = creature as MobileInstance;
					if (asMobile == null)
					{
						continue;
					}

					if (asMobile.Info.Id == Mobile.Id)
					{
						asMobile.RebuildInventory();
					}
				}
			}
		}

		public ForgeShop ForgeShop { get; set; }

		public ExchangeShop ExchangeShop { get; set; }

		[JsonIgnore]
		public bool NoCustomParams
		{
			get
			{
				return (Keywords == null || Keywords.Count == 0) &&
					string.IsNullOrEmpty(ShortDescription) &&
					string.IsNullOrEmpty(LongDescription) &&
					string.IsNullOrEmpty(Description) &&
					Guildmaster == null &&
					Shop == null &&
					ForgeShop == null &&
					ExchangeShop == null;
			}
		}

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public CreatureStats CreateStats()
		{
			var stats = new CreatureStats
			{
				HitpointsBase = Hitpoints,
				ManaBase = Mana,
				MovesBase = Moves,
				Armor = Armor
			};

			stats.Attacks.AddRange(Attacks);

			return stats;
		}

		public MobileSpawn Clone()
		{
			var clone = new MobileSpawn
			{
				Id = Id,
				Area = Area,
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Level = Level,
				Sex = Sex,
				Hitpoints = Hitpoints,
				Mana = Mana,
				Moves = Moves,
				Armor = Armor,
				Attacks = Attacks,
				Guildmaster = Guildmaster,
				Shop = Shop,
				ForgeShop = ForgeShop,
				Gold = Gold,
			};

			foreach (var word in Keywords)
			{
				clone.Keywords.Add(word);
			}

			foreach (var lootRec in Loot)
			{
				clone.Loot.Add(lootRec.Clone());
			}

			foreach (var flag in Flags)
			{
				clone.Flags.Add(flag);
			}

			foreach (var atk in SpecialAttacks)
			{
				clone.SpecialAttacks.Add(atk.Clone());
			}

			return clone;
		}

		private void InvalidateCreaturesOfThisClass()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public static MobileSpawn GetMobileById(int id) => Area.Storage.GetMobileById(id);
		public static MobileSpawn EnsureMobileById(int id) => Area.Storage.EnsureMobileById(id);
	}

	public static class MobileExtensions
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

		static MobileExtensions()
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