using System;
using AbarimMUD.Commands;
using AbarimMUD.Data;

namespace AbarimMUD
{
	public sealed class Session
	{
		private readonly Connection _connection;
		private Handler _currentHandler;
		private readonly PlayerExecutionContext _context;
		private Character _character;
		private Room _room;

		public Connection Connection
		{
			get { return _connection; }
		}

		public Handler CurrentHandler
		{
			get { return _currentHandler; }
			set
			{
				_currentHandler = value;
				if (_currentHandler != null)
				{
					_currentHandler.OnSet();
				}
			}
		}

		public Account Account { get; set; }

		public Character Character
		{
			get
			{
				return _character;
			}

			set
			{
				_character = value;

				var startArea = Database.Areas.GetById(Configuration.StartAreaName);
				_room = startArea.Rooms[Configuration.StartRoomId];
				_room.AddCharacter(_character);
			}
		}

		public Room CurrentRoom
		{
			get
			{
				return _room;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				_room.RemoveCharacter(Character);
				_room = value;
				_room.AddCharacter(Character);
			}
		}

		public PlayerExecutionContext Context
		{
			get { return _context; }
		}

		public event EventHandler Disconnected;

		public Session(Connection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}

			_connection = connection;

			CurrentHandler = new LoginHandler(this);

			_context = new PlayerExecutionContext(this);
		}

		public void ProcessInput()
		{
			if (!_connection.Alive)
			{
				return;
			}

			var input = _connection.GetInput();
			if (string.IsNullOrEmpty(input))
			{
				return;
			}

			if (CurrentHandler != null)
			{
				CurrentHandler.Process(input);
			}
		}

		public void Send(string text)
		{
			_connection.Send(text.FixNewLines());
		}

		public void Disconnect()
		{
			_connection.Disconnect();

			var ev = Disconnected;
			if (ev != null)
			{
				ev(this, EventArgs.Empty);
			}
		}
	}
}