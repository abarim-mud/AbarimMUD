using AbarimMUD.Commands.AreaBuilder;
using AbarimMUD.Commands.Owner;
using AbarimMUD.Commands.Player;
using System.Collections.Generic;
using AbarimMUD.Data;
using System.Reflection;

namespace AbarimMUD.Commands
{
	public abstract class BaseCommand
	{
		private static readonly Dictionary<string, BaseCommand> _allCommands = new Dictionary<string, BaseCommand>();

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
		public static readonly Player.Inventory Inventory = new Player.Inventory();

		public static readonly Kill Kill = new Kill();

		// Builders
		public static readonly Force Force = new Force();
		public static readonly Areas Areas = new Areas();
		public static readonly Goto Goto = new Goto();

		public static readonly RoomCreate RoomCreate = new RoomCreate();
		public static readonly RoomSet RoomSet = new RoomSet();
		public static readonly RoomLink RoomLink = new RoomLink();
		public static readonly RoomLinkClear RLinkClear = new RoomLinkClear();
		public static readonly RoomSaveResets RoomSaveResets = new RoomSaveResets();

		public static readonly MobileCreate MobileCreate = new MobileCreate();
		public static readonly MobileSpawn MSpawn = new MobileSpawn();
		public static readonly MobileSet MobileSet = new MobileSet();

		public static readonly ItemCreate ItemCreate = new ItemCreate();
		public static readonly ItemSpawn ItemSpawn = new ItemSpawn();
		public static readonly ItemSet ItemSet = new ItemSet();
		public static readonly ItemSearch ItemSearch = new ItemSearch();

		// Owner
		public static readonly SetType SetType = new SetType();
		public abstract Role RequiredType { get; }
		public static int ExecutionDepth { get; set; }

		public static IReadOnlyDictionary<string, BaseCommand> AllCommands = _allCommands;

		static BaseCommand()
		{
			// Use reflection to build dictionary of commands
			var staticFields = typeof(BaseCommand).GetFields(BindingFlags.Public | BindingFlags.Static);

			foreach(var field in staticFields)
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