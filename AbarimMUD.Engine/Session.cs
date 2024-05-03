using System;
using System.Linq;
using AbarimMUD.Commands;
using AbarimMUD.Data;
using AbarimMUD.Utils;

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

				int startRoomId;
				using (var db = Database.CreateDataContext())
				{
					startRoomId = (from r in db.Rooms where r.VNum == Configuration.StartRoomVnum select r.Id).First();
				}

				_room = Database.GetRoomById(startRoomId);
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
			_connection.Received += OnReceived;

			CurrentHandler = new LoginHandler(this);

			_context = new PlayerExecutionContext(this);
		}

		private void OnReceived(object sender, GenericEventArgs<string> e)
		{
			if (CurrentHandler != null)
			{
				CurrentHandler.Process(e.Data);
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