using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public MobileClass Class => Info.Class;

		public override string ClassName => Class.Name;

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

		public int Gold => Info.Class.GoldRange.CalculateValue(Level);

		public override event EventHandler RoomChanged;

		public MobileInstance(Mobile mobile)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));

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

		protected override CreatureStats CreateBaseStats() => Class.CreateStats(Level);

		public override string ToString() => Info.ToString();
	}
}
