using AbarimMUD.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public abstract class Creature
	{
		public static readonly List<Creature> AllCreatures = new List<Creature>();

		private CreatureStats _stats = null;

		public abstract string ShortDescription { get; }
		public abstract string Description { get; }
		public abstract GameClass Class { get; }

		public abstract int Level { get; }
		public abstract Sex Sex { get; }

		[OLCIgnore]
		public Equipment Equipment { get; set; } = new Equipment();

		[OLCIgnore]
		public Inventory Inventory { get; set; } = new Inventory();

		public CreatureStats Stats
		{
			get
			{
				UpdateStats();
				return _stats;
			}
		}

		public CreatureState State { get; } = new CreatureState();

		[JsonIgnore]
		public bool IsAlive { get; protected set; } = true;

		public void InvalidateStats()
		{
			_stats = null;
		}

		private void UpdateStats()
		{
			if (_stats != null)
			{
				return;
			}

			_stats = Class.CreateStats(Level);

			// Apply weapon to attacks
			var weapon = Equipment[SlotType.Wield];
			if (weapon != null)
			{
				int weaponPenetration, weaponMinimumDamage, weaponMaximumDamage;
				weapon.GetWeapon(out weaponPenetration, out weaponMinimumDamage, out weaponMaximumDamage);
				foreach (var attack in _stats.Attacks)
				{
					attack.AttackType = weapon.Info.AttackType;
					attack.Penetration += weaponPenetration;

					if (Class.Flags.HasFlag(GameClassFlags.Player))
					{
						// Replace damage with weapon values
						attack.MinimumDamage = weaponMinimumDamage;
						attack.MaximumDamage = weaponMaximumDamage;
					}
					else
					{
						// Add weapon values to damage
						attack.MinimumDamage += weaponMinimumDamage;
						attack.MaximumDamage += weaponMaximumDamage;
					}
				}
			}

			// Apply armor items
			foreach (var item in Equipment.Items)
			{
				if (!item.Item.ItemType.IsArmor())
				{
					continue;
				}

				int armor;
				item.Item.GetArmor(out armor);

				_stats.Armor += armor;
			}
		}

		public void Restore()
		{
			var stats = Stats;

			State.Hitpoints = stats.MaxHitpoints;
		}

		public bool? Wear(ItemInstance item)
		{
			var result = Equipment.Wear(item);

			InvalidateStats();

			return result;
		}

		public ItemInstance Remove(SlotType slot)
		{
			var result = Equipment.Remove(slot);

			InvalidateStats();

			return result;
		}

		public abstract bool MatchesKeyword(string keyword);

		public static void InvalidateAllCreaturesStats()
		{
			foreach (var creature in AllCreatures)
			{
				creature.InvalidateStats();
			}
		}

		public static void InvalidateMobiles(Mobile mobile)
		{
			foreach (var creature in AllCreatures)
			{
				var asMobile = creature as MobileInstance;
				if (asMobile == null || asMobile.Info != mobile)
				{
					continue;
				}

				asMobile.InvalidateStats();
				asMobile.Restore();
			}
		}

		public virtual void Slain()
		{
			IsAlive = false;
		}
	}
}
