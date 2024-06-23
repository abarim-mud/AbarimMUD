using AbarimMUD.Commands.Builder;
using AbarimMUD.Commands.Owner;
using AbarimMUD.Commands.Player;
using System.Collections.Generic;
using AbarimMUD.Data;
using System.Reflection;

namespace AbarimMUD.Commands
{
    public abstract class BaseCommand
	{
		public const string ExecuteName = "DoExecute";

		private static readonly Dictionary<string, BaseCommand> _allCommands = new Dictionary<string, BaseCommand>();

		// Players
		public static readonly Help Help = new Help();
		public static readonly Move East = new Move(Direction.East);
		public static readonly Move West = new Move(Direction.West);
		public static readonly Move North = new Move(Direction.North);
		public static readonly Move South = new Move(Direction.South);
		public static readonly Move Up = new Move(Direction.Up);
		public static readonly Move Down = new Move(Direction.Down);

		public static readonly Say Say = new Say();
		public static readonly Gossip Gossip = new Gossip();
		public static readonly Look Look = new Look();
		public static readonly Where Where = new Where();

		public static readonly Score Score = new Score();

		public static readonly Player.Inventory Inventory = new Player.Inventory();
		public static readonly Wear Wear = new Wear();
		public static readonly Remove Remove = new Remove();
		public static readonly Player.Equipment Equipment = new Player.Equipment();

		public static readonly Kill Kill = new Kill();
		public static readonly Backstab Backstab = new Backstab();

		// Builders
		public static readonly Peace Peace = new Peace();
		public static readonly Force Force = new Force();
		public static readonly Restore Restore = new Restore();
		public static readonly Areas Areas = new Areas();
		public static readonly Goto Goto = new Goto();

		public static readonly Show Show = new Show();
		public static readonly Info Info = new Info();
		public static readonly Set Set = new Set();
		public static readonly Create Create = new Create();
		public static readonly CreateCopy CreateCopy = new CreateCopy();
		public static readonly Spawn Spawn = new Spawn();
		public static readonly Slain Slain = new Slain();

		public static readonly ShowLevels ShowLevels = new ShowLevels();

		public static readonly RoomCreate RoomCreate = new RoomCreate();
		public static readonly RoomLink RoomLink = new RoomLink();
		public static readonly RoomLinkClear RLinkClear = new RoomLinkClear();
		public static readonly RoomSaveResets RoomSaveResets = new RoomSaveResets();

		// Owner
		public static readonly SetType SetType = new SetType();

		public abstract Role RequiredType { get; }
		public static int ExecutionDepth { get; set; }

		public static IReadOnlyDictionary<string, BaseCommand> AllCommands = _allCommands;

		static BaseCommand()
		{
			// Use reflection to build dictionary of commands
			var staticFields = typeof(BaseCommand).GetFields(BindingFlags.Public | BindingFlags.Static);

			foreach (var field in staticFields)
			{
				if (!field.FieldType.IsSubclassOf(typeof(BaseCommand)))
				{
					continue;
				}

				var name = field.Name.ToLower();
				_allCommands[name] = (BaseCommand)field.GetValue(null);
			}
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

		public void Execute(ExecutionContext context, string data = "")
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