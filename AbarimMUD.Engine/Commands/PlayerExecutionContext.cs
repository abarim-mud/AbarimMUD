using NLog;
using System;
using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public sealed class PlayerExecutionContext : ExecutionContext
	{
		private readonly Session _session;

		public override Creature Creature => _session.Character;

		public override Role Role => _session.Character.Role;

		public override Room CurrentRoom
		{
			get { return _session.CurrentRoom; }
			set { _session.CurrentRoom = value; }
		}

		public override Logger Logger
		{
			get { return LogManager.GetLogger(_session.Connection.LoggerName); }
		}

		public Session Session => _session;

		public PlayerExecutionContext(Session session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}

			_session = session;
		}

		protected override void InternalSend(string text)
		{
			_session.Send(text);
		}
	}
}