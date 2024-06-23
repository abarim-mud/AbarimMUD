using NLog;
using System.Collections.Generic;
using System.Text;
using AbarimMUD.Data;
using System.Linq;
using System;

namespace AbarimMUD.Commands
{
	public class ExecutionContext
	{
		private Creature _creature;
		private ExecutionContext _fightsWith;

		public Session Session { get; private set; }
		public Creature Creature
		{
			get => _creature;

			set
			{
				_creature = value;

				_creature.Dead += OnDead;
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

		public Room CurrentRoom
		{
			get => Creature.Room;
			set => Creature.Room = value;
		}

		public Area CurrentArea => CurrentRoom.Area;


		public bool IsStaff => Role >= Role.Builder;

		public ExecutionContext FightsWith
		{
			get => _fightsWith;

			set
			{
				if (value == _fightsWith)
				{
					return;
				}

				if (_fightsWith != null)
				{
					_fightsWith.Creature.Dead -= FightsWithIsDead;
				}

				_fightsWith = value;

				if (_fightsWith != null)
				{
					_fightsWith.Creature.Dead += FightsWithIsDead;
				}
			}
		}

		public bool IsAlive => Creature.IsAlive;

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
			if (Session == null)
			{
				return;
			}

			Session.Send(text + "\n");
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

			if (FightsWith == null)
			{
				output.Append($"<{State.Hitpoints}hp {State.Mana}ma {State.Movement}mv -> ");
			}
			else
			{
				var targetHpPercentage = FightsWith.Creature.State.Hitpoints * 100 / FightsWith.Creature.Stats.MaxHitpoints;
				output.Append($"<{State.Hitpoints}hp {State.Mana}ma {State.Movement}mv {targetHpPercentage}-> ");
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

			var command = BaseCommand.FindCommand(cmd);
			if (command == null)
			{
				Send("Arglebargle, glop-glyf!?!");
				return;
			}

			if (command.RequiredType > Role)
			{
				Send("Arglebargle, glop-glyf!?!");
				return;
			}

			var type = command.GetType();
			Log($"Command type is {type}.");

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

		private void FightsWithIsDead(object sender, EventArgs e)
		{
			FightsWith = null;
		}

		private void OnDead(object sender, EventArgs e)
		{
			FightsWith = null;
		}
	}
}