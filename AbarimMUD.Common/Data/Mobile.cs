using AbarimMUD.Attributes;
using AbarimMUD.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public enum Sex
	{
		None,
		Neutral,
		Male,
		Female,
		Either
	}

	public enum MobileFlags
	{
		Sentinel,
		NoWander,
		Bash,
		Berserk,
		Dodge,
		Kick,
		Parry,
		Trip,
		Aggressive,
		Undead,
		Wimpy,
		Healer,
		Scavenger,
		Banker,
		NoPurge,
		NoAlign,
		Pet,
		Fade,
		Changer,
		Rescue,
		Outdoors,
		Indoors,
		NoTrack,
		UpdateAlways,
		Crush,
		Fast,
		Tail,
		AssistRace,
		AssistAlign,
		AssistPlayer,
		AssistAll,
		AssistGuard,
		AssistId,
		Enchanter,

		AssistPlayers = AssistPlayer,
		IsHealer = Healer,
		IsChanger = Changer,
		AssistVNum = AssistId,
	}

	public class Mobile : AreaEntity
	{
		private MobileClass _class;
		private int _level;
		private AttackType? _attackType = null;
		private Shop _shop;

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

		public Sex Sex { get; set; }

		public MobileClass Class
		{
			get => _class;

			set
			{
				if (value == _class)
				{
					return;
				}

				_class = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public int Level
		{
			get => _level;

			set
			{
				if (value == _level)
				{
					return;
				}

				_level = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public AttackType? DefaultAttackType
		{
			get => _attackType;

			set
			{
				if (value == _attackType)
				{
					return;
				}

				_attackType = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		[OLCIgnore]
		[JsonIgnore]
		public int Gold
		{
			get
			{
				var result = Class.Gold.GetValue(Level).RoundDownToNearest(100);

				if (result > 1000)
				{
					if (result < 3000)
					{
						result = result.RoundUpToNearest(500);
					}
					else if (result < 10000)
					{
						result = result.RoundUpToNearest(1000);
					}
					else
					{
						result = result.RoundUpToNearest(5000);
					}
				}

				return result;
			}
		}

		[OLCIgnore]
		[JsonIgnore]
		public int Hitpoints => Class.Hitpoints.GetValue(Level).RoundDownToNearest(100);

		[OLCIgnore]
		[JsonIgnore]
		public int Mana => Class.Mana.GetValue(Level).RoundDownToNearest(100);

		[OLCIgnore]
		[JsonIgnore]
		public int Moves => Class.Moves.GetValue(Level);

		[OLCIgnore]
		[JsonIgnore]
		public int Armor => Class.Armor.GetValue(Level).RoundDownToNearest(10);

		[OLCIgnore]
		[JsonIgnore]
		public Attack[] Attacks
		{
			get
			{
				var result = new List<Attack>();

				foreach (var info in Class.Attacks)
				{
					if (info.MinimumLevel > Level)
					{
						continue;
					}

					var attackType = DefaultAttackType ?? AttackType.Hit;
					if (info.Type != null)
					{
						attackType = info.Type.Value;
					}

					var attackBonus = info.AttackBonus.GetValue(Level).RoundDownToNearest(10);
					var damage = new ValueRange(info.MinimumDamage.GetValue(Level), info.MaximumDamage.GetValue(Level));
					result.Add(new Attack(attackType, attackBonus, damage));
				}

				return result.ToArray();
			}
		}

		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

		public List<LootRecord> Loot { get; set; } = new List<LootRecord>();

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
				RebuildInventoriesOfThisTemplate();
			}
		}

		public ForgeShop ForgeShop { get; set; }

		public ExchangeShop ExchangeShop { get; set; }

		[OLCIgnore]
		public long Experience { get; set; }

		public Mobile()
		{
		}

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


		public long CalculateXpAward() => CreatureStats.CalculateXpAward(Hitpoints, Armor, Attacks);

		private bool IsMobileOfThisTemplate(MobileInstance mobile)
		{
			return mobile.Info.Id == Id;
		}

		private void InvalidateCreaturesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.IsMobileOfThisTemplate(asMobile))
				{
					creature.InvalidateStats();
				}
			}
		}

		private void RebuildInventoriesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null)
				{
					continue;
				}

				if (asMobile.Info.IsMobileOfThisTemplate(asMobile))
				{
					asMobile.RebuildInventory();
				}
			}
		}

		public Mobile CloneMobile()
		{
			var clone = new Mobile
			{
				ShortDescription = ShortDescription,
				LongDescription = LongDescription,
				Description = Description,
				Level = Level,
				Class = Class,
				Sex = Sex,
				DefaultAttackType = DefaultAttackType,
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

			return clone;
		}

		public bool MatchesKeyword(string keyword) => Keywords.StartsWithPattern(keyword);

		public override string ToString() => $"{ShortDescription} (#{Id})";

		public static Mobile GetMobileById(int id) => Area.Storage.GetMobileById(id);
		public static Mobile EnsureMobileById(int id) => Area.Storage.EnsureMobileById(id);
	}

	public static class MobileExtensions
	{
		public static string GetPronoun1(this Sex sex)
		{
			if (sex == Sex.Male)
			{
				return "he";
			}
			else if (sex == Sex.Female)
			{
				return "she";
			}

			return "it";
		}

		public static string GetPronoun2(this Sex sex)
		{
			if (sex == Sex.Male)
			{
				return "his";
			}
			else if (sex == Sex.Female)
			{
				return "her";
			}

			return "it's";
		}

		public static string GetPronoun3(this Sex sex)
		{
			if (sex == Sex.Male)
			{
				return "him";
			}
			else if (sex == Sex.Female)
			{
				return "her";
			}

			return "it";
		}

	}
}
