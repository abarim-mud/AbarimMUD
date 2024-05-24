using System;
using NLog;
using System.Collections.Generic;
using System.Text;
using AbarimMUD.Data;

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

		public abstract string Name { get; }
		public abstract int CurrentHP { get; }
		public abstract int CurrentIP { get; }
		public abstract int CurrentMV { get; }

		public abstract Room CurrentRoom { get; set; }
		public Area CurrentArea => CurrentRoom.Area;

		public abstract Role Role { get; }
		
		public bool IsStaff
		{
			get { return Role >= Role.Builder; }
		}

		public abstract string[] Keywords { get; }

		public abstract Logger Logger { get; }

		protected abstract void InternalSend(string text);

		public void SendTextLine(string text)
		{
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
				var context = (ExecutionContext) c.Tag;
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
				var context = (ExecutionContext) c.Tag;
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
			_sendCache.Append(string.Format("<{0}hp {1}ip {2}mv -> ", CurrentHP, CurrentIP, CurrentMV));

			// Always append color reset
			_sendCache.Append(ConsoleCommand.ColorClear);

			var data = _sendCache.ToString();

			InternalSend(data);

			_sendCache.Clear();
		}

		public Room GetRoomById(int id) => CurrentRoom.Area.GetRoomById(id);

	}
}