using AbarimMUD.Commands;
using NLog;
using System.IO;
using System.Text;

namespace AbarimMUD
{
	public sealed class GameHandler : Handler
	{
		public ExecutionContext Context { get; private set; }

		public GameHandler(Session session)
			: base(session)
		{
			Context = new ExecutionContext(session);
		}

		public override void OnSet()
		{
			var connection = Session.Connection;
			Logger.Info($"Connection {connection.RemoteIp}:{connection.RemotePort} belongs to {Session.Character.Name}");

			// Switch logger to the player logger
			OnCharacterNameChanged();

			// Execute look command
			Process("look");
		}

		public override void Process(string data) => Context.ParseAndExecute(data);

		public override void OnCharacterNameChanged()
		{
			base.OnCharacterNameChanged();

			var newLoggerName = Session.Character.BuildCharacterFolder();
			newLoggerName = Path.Combine("Data/accounts", newLoggerName);
			newLoggerName = Path.Combine(newLoggerName, "Logs/");
			Logger = LogManager.GetLogger(newLoggerName);
		}

		public override void BeforeOutputSent(StringBuilder sb)
		{
			base.BeforeOutputSent(sb);

			Context.BeforeOutputSent(sb);
		}
	}
}
