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
		protected class ModifiersAccumulator
		{
			private readonly Dictionary<ModifierType, int> _modifiers = new Dictionary<ModifierType, int>();

			public IReadOnlyDictionary<ModifierType, int> Modifiers => _modifiers;

			public List<Ability> Abilities { get; } = new List<Ability>();

			public void Add(ModifierType modifier, int value)
			{
				if (!_modifiers.ContainsKey(modifier))
				{
					_modifiers[modifier] = value;
				}
				else
				{
					_modifiers[modifier] += value;
				}
			}
		}

		public static readonly List<Creature> ActiveCreatures = new List<Creature>();

		private CreatureStats _stats = null;
		private Dictionary<string, TemporaryAffect> _temporaryAffects = new Dictionary<string, TemporaryAffect>();

		public abstract string ShortDescription { get; }
		public abstract string Description { get; }

		public abstract int Level { get; }
		public abstract Sex Sex { get; }
		public long Gold { get; set; }

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

		[JsonIgnore]
		public abstract HashSet<string> Keywords { get; }

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

		protected virtual void EnumerateModifiers(ModifiersAccumulator result)
		{
			// Eq modifiers
			// Apply equipment
			foreach (var slot in Equipment.Slots)
			{
				var item = slot.Item;
				if (item == null)
				{
					continue;
				}

				if (item.Info.Affects != null)
				{
					foreach (var pair in item.Info.Affects)
					{
						var affect = pair.Value;
						result.Add(affect.Type, affect.Value);
					}
				}

				if (item.Enchantment != null)
				{
					foreach(var pair in item.Enchantment.Affects)
					{
						result.Add(pair.Key, pair.Value);
					}
				}
			}

			// Temporary affects
			foreach (var pair in _temporaryAffects)
			{
				var affect = pair.Value.Affect;
				result.Add(affect.Type, affect.Value);
			}
		}

		protected abstract CreatureStats CreateBaseStats(int attacksCount);

		private void UpdateStats()
		{
			if (_stats != null)
			{
				return;
			}

			var modifiers = new ModifiersAccumulator();
			EnumerateModifiers(modifiers);

			// By default there's one attack
			var attacksCount = 1;

			// Apply attacks count modifiers
			int a;
			if (modifiers.Modifiers.TryGetValue(ModifierType.AttacksCount, out a))
			{
				attacksCount += a;
			}

			var weapon = Equipment.GetSlot(EquipmentSlotType.Wield).Item;
			var usesWeapon = false;
			if (weapon != null && weapon.Info.DamageRange != null)
			{
				usesWeapon = true;
				if (modifiers.Modifiers.TryGetValue(ModifierType.WeaponAttackBonus, out a))
				{
					attacksCount += a;
				}
			} else
			{
				if (modifiers.Modifiers.TryGetValue(ModifierType.MartialArtsAttacksCount, out a))
				{
					attacksCount += a;
				}

			}

			_stats = CreateBaseStats(attacksCount);

			// Apply weapon to attacks
			if (usesWeapon)
			{
				foreach (var attack in _stats.Attacks)
				{
					if (weapon.Info.AttackType != null)
					{
						attack.Type = weapon.Info.AttackType.Value;
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
			else if (this is Character)
			{
				int? martialArtsMin = null;
				int? martialArtsMax = null;

				// Apply martial arts damage range
				if (modifiers.Modifiers.TryGetValue(ModifierType.MartialArtsMinimumDamage, out a))
				{
					martialArtsMin = a;
				}

				if (modifiers.Modifiers.TryGetValue(ModifierType.MartialArtsMaximumDamage, out a))
				{
					martialArtsMax = a;
				}

				if (martialArtsMin != null || martialArtsMax != null)
				{
					foreach (var atk in _stats.Attacks)
					{
						if (martialArtsMin != null)
						{
							atk.DamageRange.Minimum = Math.Max(atk.DamageRange.Minimum, martialArtsMin.Value);
						}

						if (martialArtsMax != null)
						{
							atk.DamageRange.Maximum = Math.Max(atk.DamageRange.Maximum, martialArtsMax.Value);
						}
					}
				}
			}

				// Apply modifiers
				foreach (var pair in modifiers.Modifiers)
			{
				_stats.Apply(pair.Key, pair.Value, usesWeapon);
			}

			// Add abilities
			_stats.Abilities.AddRange(modifiers.Abilities);
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

		public ItemInstance Remove(EquipmentSlotType slot)
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
