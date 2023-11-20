using System;
using NLog;

namespace AbarimMUD
{
	public abstract class Handler
	{
		private readonly Session _session;
		protected readonly Logger _logger;

		protected Session Session
		{
			get { return _session; }
		}

		protected Handler(Session session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}

			_session = session;
			_logger = session.Connection.Logger;
		}

		public abstract void OnSet();

		public abstract void Process(string data);

		public void Send(string text)
		{
			_session.Send(text);
		}

		public void SendTextLine(string text)
		{
			_session.Send(text);
			_session.Send(ConsoleCommand.NewLine.ToString());
		}

		protected void SendDebug(string data)
		{
			_session.Send(data);
		}

		protected bool CheckPassword(string pwd)
		{
			if (string.IsNullOrEmpty(pwd))
			{
				SendTextLine("Invalid password.");
				return false;
			}

			return true;
		}
	}
}
