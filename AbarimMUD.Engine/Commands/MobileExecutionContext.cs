using System;
using System.Collections.Generic;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using NLog;

namespace AbarimMUD.Commands
{
	public sealed class MobileExecutionContext : ExecutionContext
	{
		private readonly MobileInstance _mobile;

		public MobileInstance Mobile
		{
			get { return _mobile; }
		}

		public override string Name
		{
			get { return _mobile.Info.ShortDescription; }
		}

		public override int CurrentHP
		{
			get { return 0; }
		}

		public override int CurrentIP
		{
			get { return 0; }
		}

		public override int CurrentMV
		{
			get { return 0; }
		}

		public override Role Role
		{
			get { return Role.Player; }
		}

		public override Room CurrentRoom
		{
			get { return _mobile.Room; }
			set { _mobile.Room = value; }
		}

		public override string[] Keywords
		{
			get { return _mobile.Info.Name.Split(" "); }
		}

		public override List<Attack> Attacks => new List<Attack>();

		public override int ArmorClass => 0;

		public override Logger Logger => Server.Logger;

		public MobileExecutionContext(MobileInstance mobile)
		{
			if (mobile == null)
			{
				throw new ArgumentNullException("mobile");
			}

			_mobile = mobile;
		}

		protected override void InternalSend(string text)
		{
		}
	}
}