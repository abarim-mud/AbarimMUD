using AbarimMUD.Attributes;
using AbarimMUD.Storage;
using AbarimMUD.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace AbarimMUD.Data
{
	public class MobileTemplate : IStoredInFile
	{
		public static readonly MultipleFilesStorage<MobileTemplate> Storage = new MobileTemplates();

		public const AttackType DefaultAttackType = AttackType.Hit;

		public const int DefaultAttackRating = 0;
		public const int DefaultArmor = 0;
		public const int DefaultGold = 100;
		public const int DefaultHitpoints = 100;
		public const int DefaultMana = 100;
		public const int DefaultMoves = 1000;
		public static readonly ValueRange DefaultDamageRange = new ValueRange(1, 4);

		private int _hitpoints = DefaultHitpoints, _mana = DefaultMana, _moves = DefaultMoves;
		private int _armor = DefaultArmor;
		private int _level;
		private int _gold = DefaultGold;

		public string Id { get; set; }

		[Browsable(false)]
		public HashSet<string> Keywords { get; set; } = new HashSet<string>();

		[OLCAlias("short")]
		public string ShortDescription { get; set; }

		[OLCAlias("long")]
		public string LongDescription { get; set; }

		public string Description { get; set; }

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

		public int Gold
		{
			get => _gold;

			set
			{
				if (value == _gold)
				{
					return;
				}

				_gold = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}


		public Sex Sex { get; set; }

		public int Hitpoints
		{
			get => _hitpoints;

			set
			{
				if (value == _hitpoints)
				{
					return;
				}

				_hitpoints = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public int Mana
		{
			get => _mana;

			set
			{
				if (value == _mana)
				{
					return;
				}

				_mana = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public int Moves
		{
			get => _moves;

			set
			{
				if (value == _moves)
				{
					return;
				}

				_moves = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public int Armor
		{
			get => _armor;

			set
			{
				if (value == _armor)
				{
					return;
				}

				_armor = value;
				InvalidateCreaturesOfThisTemplate();
			}
		}

		public Attack[] Attacks { get; set; }

		[Browsable(false)]
		public List<MobileSpecialAttack> SpecialAttacks { get; set; } = new List<MobileSpecialAttack>();

		[Browsable(false)]
		public HashSet<MobileFlags> Flags { get; set; } = new HashSet<MobileFlags>();

		[Browsable(false)]
		public List<LootRecord> Loot { get; set; } = new List<LootRecord>();

		private void InvalidateCreaturesOfThisTemplate()
		{
			foreach (var creature in Creature.ActiveCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null || asMobile.Info.Template == null)
				{
					continue;
				}

				if (asMobile.Info.Template.Id == Id)
				{
					creature.InvalidateStats();
				}
			}
		}

		public void Create() => Storage.Create(this);
		public void Save() => Storage.Save(this);

		public static MobileTemplate GetMobileTemplateById(string id) => Storage.GetByKey(id);
		public static MobileTemplate EnsureMobileTemplateById(string id) => Storage.EnsureByKey(id);

	}
}
