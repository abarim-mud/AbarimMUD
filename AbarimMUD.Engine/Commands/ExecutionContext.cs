using NLog;
using System.Collections.Generic;
using System.Text;
using AbarimMUD.Data;
using System.Linq;

namespace AbarimMUD.Commands
{
	public abstract class ExecutionContext
	{
		public abstract Creature Creature { get; }

		public abstract Role Role { get; }

		public abstract Logger Logger { get; }

		public string ShortDescription => Creature.ShortDescription;

		public CreatureStats Stats => Creature.Stats;
		public CreatureState State => Creature.State;

		public abstract Room CurrentRoom { get; set; }
		public Area CurrentArea => CurrentRoom.Area;


		public bool IsStaff => Role >= Role.Builder;

		protected abstract void InternalSend(string text);

		public bool MatchesKeyword(string keyword) => Creature.MatchesKeyword(keyword);

		public IEnumerable<ExecutionContext> AllExceptMe()
		{
			foreach (var s in Server.Instance.Sessions)
			{
				if (s.Context == this)
				{
					continue;
				}

				yield return s.Context;
			}
		}

		public IEnumerable<ExecutionContext> AllExceptMeInRoom()
		{
			var room = CurrentRoom;
			foreach (var c in room.Characters)
			{
				var context = (ExecutionContext)c.Tag;
				if (this == context)
				{
					continue;
				}

				yield return context;
			}
		}

		public IEnumerable<ExecutionContext> AllInRoom()
		{
			var room = CurrentRoom;
			foreach (var c in room.Characters)
			{
				var context = (ExecutionContext)c.Tag;
				yield return context;
			}
		}

		public void Send(string text = "")
		{
			InternalSend(text + "\n");
		}

		public void BeforeOutputSent(StringBuilder sb)
		{
			var text = sb.ToString();
			text = text.TrimEnd();

			// Rebuild the output, applying varios changes
			sb.Clear();

			text = text.TrimEnd();

			bool? forceDotAtTheEnd = null;
			if (!string.IsNullOrEmpty(text))
			{
				// Remove unneeded color clear at the end
				if (text.EndsWith("[reset]".ToString()))
				{
					text = text.Substring(0, text.Length - 7).TrimEnd();
				}

				var linesCount = text.Count(c => c.Equals('\n')) + 1;
				if (forceDotAtTheEnd == null)
				{
					// The default behavior is single line strings receive dot at the end
					// While multi-line string does not
					forceDotAtTheEnd = linesCount == 1;
				}
				sb.Append(text);

				if (forceDotAtTheEnd.Value && !text.EndsWith("."))
				{
					sb.Append('.');
				}

				// Always append color reset
				sb.Append("[reset]");

				// Mandatory line end
				sb.AppendLine();
			}

			// Line break before stats
			sb.AppendLine();
			sb.Append(string.Format("<{0}hp {1}ma {2}mv -> ", State.Hitpoints, State.Mana, State.Movement));
		}

		private void ParseAndExecuteLine(string line)
		{
			line = line.Trim();
			var parts = line.SplitByWhitespace(2);
			if (parts.Length == 0)
			{
				Send(string.Empty);
				return;
			}

			var cmd = parts[0];

			Logger.Info("Processing command: {0}", cmd);

			var command = BaseCommand.FindCommand(cmd);
			if (command == null)
			{
				Logger.Info("Command is unrecognized.");
				Send("Arglebargle, glop-glyf!?!");
				return;
			}

			if (command.RequiredType > Role)
			{
				Logger.Info("Command is not available for this character.");
				Send("Arglebargle, glop-glyf!?!");
				return;
			}

			var type = command.GetType();
			Logger.Info("Command type is {0}.", type);

			var args = parts.Length > 1 ? parts[1] : string.Empty;
			command.Execute(this, args);

		}

		public void ParseAndExecute(string data)
		{
			var lines = data.Split("\n");
			foreach (var line in lines)
			{
				ParseAndExecuteLine(line);
			}
		}
	}
}