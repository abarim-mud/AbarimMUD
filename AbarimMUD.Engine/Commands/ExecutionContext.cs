using System.Collections.Generic;
using System.Text;
using AbarimMUD.Data;
using System.Linq;
using System;
using AbarimMUD.Combat;

namespace AbarimMUD.Commands
{
	public class ExecutionContext
	{
		private Creature _creature;
		private Queue<string> _commandQueue = new Queue<string>();
		private DateTime? _commandLagStart = null;
		private int _lagInMs = 0;

		public Session Session { get; private set; }
		public Creature Creature
		{
			get => _creature;

			private set
			{
				_creature = value;

				_creature.Dead += OnDead;
				_creature.RoomChanged += OnRoomChanged;
			}
		}

		public Role Role
		{
			get
			{
				var asCharacter = Creature as Character;
				if (asCharacter != null)
				{
					return asCharacter.Role;
				}

				return Role.Player;
			}
		}

		public string ShortDescription => Creature.ShortDescription;

		public CreatureStats Stats => Creature.Stats;
		public CreatureState State => Creature.State;

		public Room Room
		{
			get => Creature.Room;
			set => Creature.Room = value;
		}

		public Area CurrentArea => Room.Area;


		public bool IsStaff => Role >= Role.Builder;

		public FightInfo FightInfo { get; } = new FightInfo();
		public bool IsFighting => FightInfo.Fight != null;

		public bool IsAlive => Creature.IsAlive;
		public int Level => Creature.Level;

		public bool HasCommands => _commandQueue.Count > 0;

		public ExecutionContext(Session session)
		{
			Session = session ?? throw new ArgumentNullException(nameof(session));
			Creature = Session.Character;
			Session.Character.Tag = this;
		}

		public ExecutionContext(MobileInstance mobile)
		{
			Creature = mobile ?? throw new ArgumentException(nameof(Creature));
			mobile.Tag = this;
		}

		public bool MatchesKeyword(string keyword) => Creature.MatchesKeyword(keyword);

		public IEnumerable<ExecutionContext> AllExceptMe()
		{
			foreach (var s in Server.Instance.Sessions)
			{
				if (s == Session)
				{
					continue;
				}

				var asGameHandler = s.CurrentHandler as GameHandler;
				if (asGameHandler == null)
				{
					continue;
				}

				yield return asGameHandler.Context;
			}
		}

		public IEnumerable<ExecutionContext> AllExceptMeInRoom()
		{
			var room = Room;
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
			var room = Room;
			foreach (var c in room.Characters)
			{
				var context = (ExecutionContext)c.Tag;
				yield return context;
			}
		}

		public void Send(string text = "")
		{
			if (Session == null)
			{
				return;
			}

			Session.Send(text + "\n");
		}

		public void SendInfoMessage(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				Send();
				return;
			}

			Send("[green]" + text + "[reset]");
		}

		public void SendBattleMessage(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				Send();
				return;
			}

			Send("[yellow]" + text + "[reset]");
		}

		private static string BuildStat(int value, int maxValue, bool percValue = false)
		{
			string color = "green";
			var perc = value * 100 / maxValue;

			if (perc < 25)
			{
				color = "red";
			} else if (perc < 80)
			{
				color = "yellow";
			}

			if (percValue)
			{
				value = perc;
			}

			return $"[{color}]{value}[reset]";
		}

		public void BeforeOutputSent(StringBuilder output)
		{
			var text = output.ToString();
			text = text.TrimEnd();

			// Rebuild the output, applying varios changes
			output.Clear();

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
				output.Append(text);

				if (forceDotAtTheEnd.Value && !text.EndsWith("."))
				{
					output.Append('.');
				}

				// Always append color reset
				output.Append("[reset]");

				// Mandatory line end
				output.AppendLine();
			}

			// Line break before stats
			output.AppendLine();


			output.Append($"<{BuildStat(State.Hitpoints, Stats.MaxHitpoints)}hp {BuildStat(State.Mana, Stats.MaxMana)}ma {BuildStat(State.Moves, Stats.MaxMoves)}mv ");

			var target = FightInfo.Target;
			if (target == null)
			{
				output.Append("-> ");
			}
			else
			{
				output.Append($"{BuildStat(target.Creature.State.Hitpoints, target.Creature.Stats.MaxHitpoints, true)}-> ");
			}
		}

		private void Log(string message)
		{
			if (Session == null)
			{
				return;
			}

			Session.Logger.Info(message);
		}

		public bool WaitingCommandLag()
		{
			if (_commandLagStart == null)
			{
				return false;
			}

			if ((DateTime.Now - _commandLagStart.Value).TotalMilliseconds < _lagInMs)
			{
				return true;
			}

			_commandLagStart = null;
			_lagInMs = 0;
			
			return false;
		}

		public void ProcessCommandQueue()
		{
			while (!WaitingCommandLag() && _commandQueue.Count > 0)
			{
				var line = _commandQueue.Dequeue();
				line = line.Trim();
				var parts = line.SplitByWhitespace(2);
				if (parts.Length == 0)
				{
					Send(string.Empty);
					continue;
				}

				var cmd = parts[0];

				var command = BaseCommand.FindCommand(cmd);
				if (command == null)
				{
					Send("Arglebargle, glop-glyf!?!");
					continue;
				}

				if (command.RequiredType > Role)
				{
					Send("Arglebargle, glop-glyf!?!");
					continue;
				}

				var type = command.GetType();
				Log($"Command type is {type}.");

				var args = parts.Length > 1 ? parts[1] : string.Empty;
				command.Execute(this, args);
			}
		}

		public void StartCommandLag(int lagInMs)
		{
			if (lagInMs <= 0)
			{
				return;
			}

			_commandLagStart = DateTime.Now;
			_lagInMs = lagInMs;
		}

		public void ParseAndExecute(string data)
		{
			var lines = data.Split("\n");
			foreach (var line in lines)
			{
				if (line.Trim() == "|")
				{
					// Special command used to clear the command queue
					_commandQueue.Clear();
					Send("Cleared the command queue.");
				}
				else
				{
					_commandQueue.Enqueue(line);
				}
			}

			ProcessCommandQueue();
		}

		public void LeaveFight()
		{
			if (FightInfo.Fight == null)
			{
				return;
			}

			FightInfo.Fight.LeaveFight(this);
		}

		private void OnRoomChanged(object sender, EventArgs e)
		{
			LeaveFight();
		}

		private void OnDead(object sender, EventArgs e)
		{
			LeaveFight();
		}
	}
}