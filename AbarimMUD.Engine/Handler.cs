using AbarimMUD.Utils;
using NLog;
using System;
using System.Text;

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

		public void SendLine(string text)
		{
			Session.Send(text + AsciiRichText.NewLine);
		}

		public virtual void BeforeOutputSent(StringBuilder output)
		{
		}

		protected bool CheckPassword(string pwd)
		{
			if (string.IsNullOrEmpty(pwd))
			{
				SendLine("Invalid password.");
				return false;
			}

			return true;
		}

		public virtual void OnCharacterNameChanged()
		{
		}
	}
}
