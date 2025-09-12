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
			Send($"{_previousSession.Account.Name}/{_previousSession.Character.Name} is in-game. Would you like to reconnect(Y/n)?");
		}

		public override void Process(string data)
		{
			if (data.EqualsToIgnoreCase("n"))
			{
				// No
				SendLine("Bye");
				Session.Disconnect();
				return;
			}

			// Yes
			// Copy params
			Session.Account = _previousSession.Account;
			Session.Character = _previousSession.Character;

			// Clone handler
			var handlerType = _previousSession.CurrentHandler.GetType();
			var newHandler = (Handler)Activator.CreateInstance(handlerType, Session);
			Session.CurrentHandler = newHandler;

			// Disconnect previous session
			_previousSession.Disconnect();
		}
	}
}