using NLog;
using System;
using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public sealed class PlayerExecutionContext : ExecutionContext
	{
		private readonly Session _session;

		public override string Name
		{
			get { return _session.Character.Name; }
		}

		public override Role Role
		{
			get { return _session.Character.Role; }
		}

		public override int CurrentHP
		{
			get { return _session.Character.CurrentHP; }
		}

		public override int CurrentIP
		{
			get { return _session.Character.CurrentIP; }
		}

		public override int CurrentMV
		{
			get { return _session.Character.CurrentIP; }
		}
	
		public override Room CurrentRoom
		{
			get { return _session.CurrentRoom; }
			set { _session.CurrentRoom = value; }
		}

		public override Logger Logger
		{
			get { return LogManager.GetLogger(_session.Connection.LoggerName); }
		}

		public override string[] Keywords
		{
			get { return new[] {_session.Character.Name.ToLower()}; }
		}

		public Session Session
		{
			get { return _session; }
		}

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