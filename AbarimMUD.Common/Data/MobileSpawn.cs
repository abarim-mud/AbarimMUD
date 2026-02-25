using System;
using System.ComponentModel;

namespace AbarimMUD.Data
{
	public class MobileSpawn
	{
		private Mobile _mobile;

		[Browsable(false)]
		public Mobile Mobile
		{
			get => _mobile;

			set
			{
				_mobile = value ?? throw new ArgumentNullException(nameof(value));
			}
		}

		[Browsable(false)]
		public Room Room { get; internal set; }

		[Browsable(false)]
		public MobileInstance Instance { get; internal set; }
	}
}