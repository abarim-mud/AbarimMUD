﻿using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public int MaxHitpoints { get; }
		public int MaxMana { get; }
		public int MaxMoves { get; }

		public HashSet<string> Keywords => Info.Keywords;
		public override string ShortDescription => Info.ShortDescription;
		public override string Description => Info.Description;

		public override int Level => Info.Level;

		public override Sex Sex => Info.Sex;

		public override Room Room
		{
			get { return _room; }

			set
			{
				if (_room != null)
				{
					_room.Mobiles.Remove(this);
				}

				_room = value;

				if (_room != null)
				{
					_room.Mobiles.Add(this);
				}

				RoomChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public override event EventHandler RoomChanged;

		public MobileInstance(Mobile mobile)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));
			Gold = Info.Gold;
			MaxHitpoints = Info.HitpointsRange.Random();
			MaxMana = Info.ManaRange.Random();
			MaxMoves = Info.MovesRange.Random();

			Restore();

			ActiveCreatures.Add(this);
		}

		public override bool MatchesKeyword(string keyword) => Info.MatchesKeyword(keyword);

		protected override void Slain()
		{
			base.Slain();
			ActiveCreatures.Remove(this);
			Room = null;
		}

		protected override CreatureStats CreateBaseStats(int attacksCount)
		{
			// Mobiles ignore attacksCount, since their attacks are set explicitly
			var result = Info.CreateStats();

			result.MaxHitpoints = MaxHitpoints;
			result.MaxMana = MaxMana;
			result.MaxMoves = MaxMoves;

			return result;
		}

		public override string ToString() => Info.ToString();
	}
}
