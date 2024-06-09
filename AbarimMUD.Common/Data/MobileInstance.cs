using System;

namespace AbarimMUD.Data
{
	public class MobileInstance : Creature
	{
		private Room _room;

		public Mobile Info { get; }

		public override string Name => Info.Name;

		public override Race Race => Info.Race;

		public override GameClass Class => Info.Class;

		public override int Level => Info.Level;

		public override Sex Sex => Info.Sex;

		public Room Room
		{
			get { return _room; }

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				if (_room != null)
				{
					_room.Mobiles.Remove(this);
				}

				_room = value;
				_room.Mobiles.Add(this);
			}
		}
		public MobileInstance(Mobile mobile)
		{
			Info = mobile ?? throw new ArgumentNullException(nameof(mobile));

			Restore();

			AllCreatures.Add(this);
		}
	}
}
