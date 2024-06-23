using System;

namespace AbarimMUD.Data
{
	public class CreatureState
	{
		private int _hitpoints;

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

		public float FractionalRegen { get; set; }

		public int Mana { get; set; }

		public int Movement { get; set; }

		public event EventHandler HitpointsChanged;
	}
}
