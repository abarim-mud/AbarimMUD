using AbarimMUD.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AbarimMUD.Data
{
	public class TemporaryAffect
	{
		public string Name { get; }
		public Affect Affect { get; }
		public DateTime Started { get; }

		public TemporaryAffect(string name, Affect affect)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			if (affect == null)
			{
				throw new ArgumentNullException(nameof(affect));
			}

			if (affect.DurationInSeconds == null)
			{
				throw new ArgumentException("affect.DurationInSeconds isn't set");
			}

			Name = name;
			Affect = affect;
			Started = DateTime.Now;
		}
	}

	public abstract class Creature
	{
		public static readonly List<Creature> ActiveCreatures = new List<Creature>();

		private CreatureStats _stats = null;
		private Dictionary<string, TemporaryAffect> _temporaryAffects = new Dictionary<string, TemporaryAffect>();

		public abstract string ShortDescription { get; }
		public abstract string Description { get; }

		public abstract string ClassName { get; }
		public abstract int Level { get; }
		public abstract Sex Sex { get; }

		[JsonIgnore]
		[OLCIgnore]
		public abstract Room Room { get; set; }

		[OLCIgnore]
		public Equipment Equipment { get; set; } = new Equipment();

		[OLCIgnore]
		public Inventory Inventory { get; set; } = new Inventory();

		[JsonIgnore]
		public CreatureStats Stats
		{
			get
			{
				UpdateStats();
				return _stats;
			}
		}

		[JsonIgnore]
		public CreatureState State { get; } = new CreatureState();

		[JsonIgnore]
		public bool IsAlive => State.Hitpoints >= 0;

		[JsonIgnore]
		public IReadOnlyDictionary<string, TemporaryAffect> TemporaryAffects => _temporaryAffects;

		[JsonIgnore]
		public object Tag { get; set; }

		public event EventHandler Dead;
		public abstract event EventHandler RoomChanged;

		public Creature()
		{
			State.HitpointsChanged += (s, e) =>
			{
				if (State.Hitpoints < 0)
				{
					Slain();
					Dead?.Invoke(this, EventArgs.Empty);
				}
			};
		}

		public void InvalidateStats()
		{
			_stats = null;
		}

		private static void ApplyModifier(ModifierType type, int val, CreatureStats stats)
		{
			switch (type)
			{
				case ModifierType.AttacksCount:
					stats.Attacks.Add(stats.Attacks[0].Clone());
					break;
				case ModifierType.WeaponPenetration:
					foreach(var atk in stats.Attacks)
					{
						atk.Penetration += val;
					}
					break;
				case ModifierType.BackstabCount:
					stats.BackstabCount += val;
					break;
				case ModifierType.BackstabMultiplier:
					stats.BackstabMultiplier += val;
					break;
				case ModifierType.Armor:
					stats.Armor += val;
					break;
				case ModifierType.HpRegen:
					stats.HpRegenBonus += val;
					break;
				case ModifierType.ManaRegen:
					stats.ManaRegenBonus += val;
					break;
				case ModifierType.MoveRegen:
					stats.MovesRegenBonus += val;
					break;
			}
		}


		protected abstract CreatureStats CreateBaseStats();

		private void UpdateStats()
		{
			if (_stats != null)
			{
				return;
			}

			_stats = CreateBaseStats();

			// Apply weapon to attacks
			var weapon = Equipment[SlotType.Wield];
			if (weapon != null && weapon.Info.DamageRange != null)
			{
				foreach (var attack in _stats.Attacks)
				{
					if (weapon.Info.AttackType != null)
					{
						attack.AttackType = weapon.Info.AttackType.Value;
					}

					if (this is Character)
					{
						// Player
						// Replace damage with weapon values
						attack.DamageRange = weapon.Info.DamageRange.Value;
					}
					else
					{
						// Mobile
						// Add weapon values to damage
						attack.DamageRange += weapon.Info.DamageRange.Value;
					}
				}
			}

			// Temporary affects
			foreach(var pair in _temporaryAffects)
			{
				var affect = pair.Value.Affect;
				ApplyModifier(affect.Type, affect.Value, _stats);
			}
		}

		public void Restore()
		{
			var stats = Stats;

			State.Hitpoints = stats.MaxHitpoints;
			State.Mana = stats.MaxMana;
			State.Moves = stats.MaxMoves;
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
			foreach (var creature in ActiveCreatures)
			{
				creature.InvalidateStats();
			}
		}

		public static void InvalidateMobiles(Mobile mobile)
		{
			foreach (var creature in ActiveCreatures)
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

		public void AddTemporaryAffect(string slot, string name, Affect affect)
		{
			_temporaryAffects[slot] = new TemporaryAffect(name, affect);
			InvalidateStats();
		}

		public void RemoveTemporaryAffect(string slot)
		{
			if (_temporaryAffects.ContainsKey(slot))
			{
				_temporaryAffects.Remove(slot);
				InvalidateStats();
			}
		}

		protected virtual void Slain()
		{
		}
	}
}
