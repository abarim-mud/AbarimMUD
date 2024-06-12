using System;
using AbarimMUD.Data;
using NLog;

namespace AbarimMUD.Commands
{
	public sealed class MobileExecutionContext : ExecutionContext
	{
		private readonly MobileInstance _mobile;

		public override Creature Creature => _mobile;

		public override Role Role
		{
			get { return Role.Player; }
		}

		public override Room CurrentRoom
		{
			get { return _mobile.Room; }
			set { _mobile.Room = value; }
		}

		public override Logger Logger => Server.Logger;

		public MobileInstance Mobile => _mobile;

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