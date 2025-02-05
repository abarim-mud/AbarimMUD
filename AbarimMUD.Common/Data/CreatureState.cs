using System;

namespace AbarimMUD.Data
{
	public class CreatureState
	{
		private int _hitpoints, _mana, _moves;

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
				HitpointsChanged?.Invoke(this, EventArgs.Empty);
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
				ManaChanged?.Invoke(this, EventArgs.Empty);
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
				MovesChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public float FractionalHitpointsRegen { get; set; }
		public float FractionalManaRegen { get; set; }
		public float FractionalMovesRegen { get; set; }

		public event EventHandler HitpointsChanged;
		public event EventHandler ManaChanged;
		public event EventHandler MovesChanged;
	}
}
