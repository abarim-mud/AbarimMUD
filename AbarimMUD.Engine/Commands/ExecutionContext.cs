using System;
using NLog;
using System.Collections.Generic;
using System.Text;
using AbarimMUD.Data;
using AbarimMUD.Utils;

namespace AbarimMUD.Commands
{
	public abstract class ExecutionContext
	{
		public sealed class SendSuspender : IDisposable
		{
			private readonly ExecutionContext _session;

			public SendSuspender(ExecutionContext session)
			{
				if (session == null)
				{
					throw new ArgumentNullException("session");
				}

				_session = session;
				_session._suspendSend = true;
			}

			public void Dispose()
			{
				_session._suspendSend = false;
				_session.Flush();
			}
		}

		private bool _suspendSend;
		private readonly StringBuilder _sendCache = new StringBuilder();

		public abstract Creature Creature { get; }

		public abstract Role Role { get; }

		public abstract string[] Keywords { get; }

		public abstract Logger Logger { get; }

		public string Name => Creature.Name;
		public CreatureStats Stats => Creature.Stats;
		public CreatureState State => Creature.State;
		public Attack[] Attacks => Stats.Attacks;
		public int ArmorClass => Stats.Armor;

		public abstract Room CurrentRoom { get; set; }
		public Area CurrentArea => CurrentRoom.Area;


		public bool IsStaff => Role >= Role.Builder;

		protected abstract void InternalSend(string text);

		public void SendTextLine(string text)
		{
			if (!text.EndsWith("."))
			{
				text += ".";
			}

			InternalSend(text + ConsoleCommand.NewLine);
		}

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

		public void Send(string text)
		{
			_sendCache.Append(text);
			Flush();
		}

		private void Flush()
		{
			if (_suspendSend)
			{
				return;
			}

			_sendCache.AddNewLine();
			_sendCache.Append(string.Format("<{0}hp {1}ma {2}mv -> ", State.Hitpoints, State.Mana, State.Movement));

			// Always append color reset
			_sendCache.Append(ConsoleCommand.ColorClear);

			var data = _sendCache.ToString();

			InternalSend(data);

			_sendCache.Clear();
		}

		public void ParseAndExecute(string data)
		{
			var parts = data.SplitByWhitespace(2);
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

			Logger.Info("Command type is {0}.", cmd.GetType());

			var args = parts.Length > 1 ? parts[1] : string.Empty;
			command.Execute(this, args);
		}
	}
}