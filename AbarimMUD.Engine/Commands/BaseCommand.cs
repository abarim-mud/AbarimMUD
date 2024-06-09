using AbarimMUD.Commands.AreaBuilder;
using AbarimMUD.Commands.Owner;
using AbarimMUD.Commands.Player;
using System.Collections.Generic;
using AbarimMUD.Data;

namespace AbarimMUD.Commands
{
	public abstract class BaseCommand
	{
		private static readonly Dictionary<string, BaseCommand> _allCommands = new Dictionary<string, BaseCommand>();

		public abstract Role RequiredType { get; }
		public static int ExecutionDepth { get; set; }

		public static Dictionary<string, BaseCommand> AllCommands
		{
			get { return _allCommands; }
		}

		static BaseCommand()
		{
			_allCommands["help"] = new Help();

			_allCommands["east"] = new Move(Direction.East);
			_allCommands["west"] = new Move(Direction.West);
			_allCommands["north"] = new Move(Direction.North);
			_allCommands["south"] = new Move(Direction.South);
			_allCommands["up"] = new Move(Direction.Up);
			_allCommands["down"] = new Move(Direction.Down);

			_allCommands["say"] = new Say();
			_allCommands["gossip"] = new Gossip();
			_allCommands["look"] = new Look();
			_allCommands["where"] = new Where();

			_allCommands["kill"] = new Kill();

			// Builders
			_allCommands["force"] = new Force();
			_allCommands["areas"] = new Areas();
			_allCommands["rset"] = new RSet();
			_allCommands["rlink"] = new RLink();
			_allCommands["rlinkclear"] = new RLinkClear();
			_allCommands["rsaveresets"] = new RSaveResets();

			_allCommands["goto"] = new Goto();
			_allCommands["rcreate"] = new RCreate();
			_allCommands["mcreate"] = new MCreate();
			_allCommands["mspawn"] = new MSpawn();
			_allCommands["mset"] = new MSet();

			// Owner
			_allCommands["settype"] = new SetType();
		}

		public static BaseCommand FindCommand(string name)
		{
			name = name.ToLower();
			foreach (var ac in _allCommands)
			{
				if (ac.Key.StartsWith(name))
				{
					return ac.Value;
				}
			}

			// Not found
			return null;
		}

		public static void ParseAndExecute(ExecutionContext context, string data)
		{
			data = data.Trim();
			if (string.IsNullOrEmpty(data))
			{
				context.Send(string.Empty);
				return;
			}

			string cmdText, cmdData;
			data.ParseCommand(out cmdText, out cmdData);

			context.Logger.Info("Processing command: {0}", cmdText);
			if (string.IsNullOrEmpty(cmdText))
			{
				return;
			}

			var command = FindCommand(cmdText);
			if (command == null)
			{
				context.Logger.Info("Command is unrecognized.");
				context.Send("Arglebargle, glop-glyf!?!");
				return;
			}

			if (command.RequiredType > context.Role)
			{
				context.Logger.Info("Command is not available for this character.");
				context.Send("Arglebargle, glop-glyf!?!");
				return;
			}

			context.Logger.Info("Command type is {0}.", command.GetType());

			command.Execute(context, cmdData);
		}

		public void Execute(ExecutionContext context, string data)
		{
			try
			{
				++ExecutionDepth;
				InternalExecute(context, data);
			}
			finally
			{
				--ExecutionDepth;
			}

		}

		protected abstract void InternalExecute(ExecutionContext context, string data);
	}
}