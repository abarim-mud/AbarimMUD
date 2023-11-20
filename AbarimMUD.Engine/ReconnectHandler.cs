using System;

namespace AbarimMUD
{
	public sealed class ReconnectHandler : Handler
	{
		private readonly Session _previousSession;

		public Session PreviousSession
		{
			get { return _previousSession; }
		}

		public ReconnectHandler(Session session, Session previousSession)
			: base(session)
		{
			if (previousSession == null)
			{
				throw new ArgumentNullException("previousSession");
			}

			_previousSession = previousSession;
		}

		public override void OnSet()
		{
			Send(string.Format("Account {0} is already connected. Would you like to reconnect(Y/n)?",
				_previousSession.Account.Id));
		}

		public override void Process(string data)
		{
			if (string.Compare("n", data, StringComparison.OrdinalIgnoreCase) == 0)
			{
				// No
				SendTextLine("Bye");
				Session.Disconnect();
				return;
			}

			// Yes
			// Copy params
			Session.Account = _previousSession.Account;
			Session.Character = _previousSession.Character;

			// Clone handler
			var handlerType = _previousSession.CurrentHandler.GetType();
			var newHandler = (Handler) Activator.CreateInstance(handlerType, Session);
			Session.CurrentHandler = newHandler;

			// Disconnect previous session
			_previousSession.Disconnect();
		}
	}
}