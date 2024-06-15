using System;
using System.Collections.Generic;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public HashSet<string> Keywords => Info.Keywords;
		public override string ShortDescription => Info.ShortDescription;
		public override string Description => Info.Description;

		public override GameClass Class => Info.Class;

		public override int Level => Info.Level;

		public override Sex Sex => Info.Sex;

		public Room Room
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
			}
		}
		public MobileInstance(Mobile mobile)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));

			Restore();

			AllCreatures.Add(this);
		}

		public override bool MatchesKeyword(string keyword) => Info.MatchesKeyword(keyword);


		public override void Slain()
		{
			base.Slain();

			Room = null;
		}
	}
}
