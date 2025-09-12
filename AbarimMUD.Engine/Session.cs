using System;
using System.Text;
using AbarimMUD.Data;
using NLog;

namespace AbarimMUD
{
	public sealed class Session
	{
		private readonly Connection _connection;
		private Handler _currentHandler;
		private Character _character;
		private readonly StringBuilder _output = new StringBuilder();
		private bool _hasOutput = false;

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
				_character.Restore();
				_character.Room = Room.EnsureRoomById(Configuration.StartRoomId);
				Creature.ActiveCreatures.Add(_character);
				Character.ActiveCharacters.Add(_character);
			}
		}

		public event EventHandler Disconnected;

		public Session(Connection connection, bool firstSession = false)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}

			_connection = connection;

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
					Account.LastLogin = DateTime.UtcNow;
					Account.Save();
					
					Character = character;
					Character.LastLogin = DateTime.UtcNow;
					Character.Save();
					
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
			_output.Append(text);
			_hasOutput = true;
		}

		public void FlushOutput()
		{
			if (!_hasOutput)
			{
				return;
			}

			CurrentHandler.BeforeOutputSent(_output);
			_connection.Send(_output.ToString());
			_output.Clear();
			_hasOutput = false;
		}

		public void Disconnect()
		{
			if (_connection.Alive)
			{
				FlushOutput();
			}

			if (_character != null)
			{
				Creature.ActiveCreatures.Remove(_character);
				Character.ActiveCharacters.Remove(_character);
			}

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
			foreach (var session in Server.Instance.Sessions)
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