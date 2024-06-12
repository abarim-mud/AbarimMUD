using AbarimMUD.Commands;
using NLog;
using System.IO;

namespace AbarimMUD
{
	public sealed class GameHandler : Handler
	{
		public GameHandler(Session session)
			: base(session)
		{
		}

		public override void OnSet()
		{
			var connection = Session.Connection;
			Logger.Info($"Connection {connection.RemoteIp}:{connection.RemotePort} belongs to {Session.Character.Name}");

			// Switch logger to the player logger
			var newLoggerName = Session.Character.BuildCharacterFolder();
			newLoggerName = Path.Combine("Data/accounts", newLoggerName);
			newLoggerName = Path.Combine(newLoggerName, "Logs/");
			Logger = LogManager.GetLogger(newLoggerName);

			// Execute look command
			Process("look");
		}

		/*		public override void BeforeSend(StringBuilder sb)
				{
					base.BeforeSend(sb);

					// Add prompt if execution depth is 1(top command)
					var c = Session.Character;

					sb.Append(ConsoleCommand.NewLine);
					sb.Append(string.Format("<{0}hp {1}ip {2}mv -> ", c.CurrentHP, c.CurrentIP, c.CurrentMV));
				}*/

		public override void Process(string data) => Session.Context.ParseAndExecute(data);
	}
}
