using NLog;
using System;

namespace AbarimMUD
{
	public abstract class Handler
	{
		protected Session Session { get; private set; }
		protected Logger Logger
		{
			get => Session.Logger;
			set => Session.Logger = value;
		}

		protected Handler(Session session)
		{
			if (session == null)
			{
				throw new ArgumentNullException("session");
			}

			Session = session;
		}

		public abstract void OnSet();

		public abstract void Process(string data);

		public void Send(string text)
		{
			Session.Send(text);
		}

		public void SendTextLine(string text)
		{
			Session.Send(text);
			Session.Send(ConsoleCommand.NewLine.ToString());
		}

		protected void SendDebug(string data)
		{
			Session.Send(data);
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
