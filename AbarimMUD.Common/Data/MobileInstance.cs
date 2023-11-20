using System;

namespace AbarimMUD.Data
{
	public class MobileInstance
	{
		private Room _room;

		public Mobile Info { get; }
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
		}
	}
}
