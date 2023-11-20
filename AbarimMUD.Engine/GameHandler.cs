using AbarimMUD.Commands;
using ExecutionContext = AbarimMUD.Commands.ExecutionContext;

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

		public override void Process(string data)
		{
			using (new ExecutionContext.SendSuspender(Session.Context))
			{
				BaseCommand.ParseAndExecute(Session.Context, data);
			}
		}
	}
}
