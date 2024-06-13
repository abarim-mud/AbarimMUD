using System;
using AbarimMUD.Commands;
using AbarimMUD.Data;
using NLog;

namespace AbarimMUD
{
	public sealed class Session
	{
		private readonly Connection _connection;
		private Handler _currentHandler;
		private readonly PlayerExecutionContext _context;
		private Character _character;
		private Room _room;

		public Connection Connection => _connection;

		public Logger Logger
		{
			get => _connection.Logger;
			set => _connection.Logger = value;
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
				_character.Tag = Context;

				_character.Restore();

				_room = Room.EnsureRoomById(Configuration.StartRoomId);
				_room.AddCharacter(_character);
				Creature.AllCreatures.Add(_character);
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

		public Session(Connection connection, bool firstSession = false)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}

			_connection = connection;
			_context = new PlayerExecutionContext(this);

			if (!firstSession || string.IsNullOrEmpty(Configuration.DefaultCharacter))
			{
				CurrentHandler = new LoginHandler(this);
			}
			else
			{
				var character = Character.GetCharacterByName(Configuration.DefaultCharacter);
				if (character == null)
				{
					Logger.Error($"Unable to find default character {Configuration.DefaultCharacter}");
					CurrentHandler = new LoginHandler(this);
				}
				else
				{
					Logger.Info($"Logging {Configuration.DefaultCharacter} since it is first session and default character is set.");
					Account = character.Account;
					Character = character;
					CurrentHandler = new GameHandler(this);
				}
			}
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

			// Trim newline at the end
			input = input.TrimEnd();
			if (CurrentHandler != null)
			{
				CurrentHandler.Process(input);
			}
		}

		public void Send(string text)
		{
			_connection.Send(text);
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

		public void OnCharacterNameChanged()
		{
			CurrentHandler?.OnCharacterNameChanged();
		}

		public static Session FindSessionByCharacterName(string name)
		{
			foreach(var session in Server.Instance.Sessions)
			{
				if (session.Character != null && session.Character.Name.EqualsToIgnoreCase(name))
				{
					return session;
				}
			}

			return null;
		}
	}
}