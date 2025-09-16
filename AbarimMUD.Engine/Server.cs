using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using AbarimMUD.WebService;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using System.Collections.Generic;
using AbarimMUD.Combat;
using System.Threading;
using AbarimMUD.Commands;

namespace AbarimMUD
{
	public sealed class Server
	{
		private static readonly Server _instance = new Server();
		private readonly List<Connection> _newConnections = new List<Connection>();
		private readonly ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
		private Session[] _sessionsCopy = null;
		private readonly AutoResetEvent _mainThreadEvent = new AutoResetEvent(false);
		private readonly Service _webService = new Service();
		private DateTime? _lastRegenDt;
		private readonly List<string> _toDelete = new List<string>();

		public static Logger Logger { get; private set; } = LogManager.GetLogger("Logs/Server");

		public static Server Instance => _instance;

		public Session[] Sessions
		{
			get
			{
				if (_sessionsCopy != null)
				{
					return _sessionsCopy;
				}

				lock (_sessions)
				{
					_sessionsCopy = _sessions.ToArray();
				}

				return _sessionsCopy;
			}
		}

		private Server()
		{
			_sessions.CollectionChanged += (s, e) =>
			{
				_sessionsCopy = null;
			};
		}

		private void LoadDatabase(string dataFolder)
		{
			Logger.Info("Loading Database");

			StorageUtility.InitializeStorage(Logger.Info);

			DataContext.Load(dataFolder);

/*			Area.Storage.SaveAll();
			PlayerClass.Storage.SaveAll();
			Skill.Storage.SaveAll();
			Ability.Storage.SaveAll();
			Configuration.Save();
			SkillCostInfo.Storage.SaveAll();
			Item.Storage.SaveAll();
			GenericLoot.Save();
			Shop.Storage.SaveAll();
			ForgeShop.Storage.SaveAll();
			ExchangeShop.Storage.SaveAll();
			Enchantment.Storage.SaveAll();
			MobileTemplate.Storage.SaveAll();
			LevelInfo.Storage.SaveAll();*/
		}

		private bool ProcessRegen(ref int currentValue, int maxValue, ref float fractionalValue, int regenValue, float secondsPassed)
		{
			if (currentValue == maxValue)
			{
				return false;
			}

			float r;
			if (currentValue < maxValue)
			{
				r = regenValue * secondsPassed / 60.0f;
			}
			else
			{
				r = -Configuration.NegativeRegen * secondsPassed / 60.0f;
			}

			fractionalValue += r;
			if (Math.Abs(fractionalValue) > 1)
			{
				// Update real hp
				var hpUpdate = (int)fractionalValue;
				currentValue += hpUpdate;
				fractionalValue -= hpUpdate;
				if (currentValue >= maxValue)
				{
					// Full
					currentValue = maxValue;
					fractionalValue = 0;
				}
			}

			return true;
		}

		private void ProcessHunt(Commands.ExecutionContext ctx)
		{
			var now = DateTime.Now;

			if (!ctx.HuntInfo.IsActive || (now - ctx.HuntInfo.LastHunt).TotalMilliseconds < Configuration.HuntPauseInMs)
			{
				return;
			}

			var target = ctx.HuntInfo.Target;
			if (target.Room.Id == ctx.Room.Id)
			{
				// Found
				ctx.Send($"The hunt is over. You found {target.ShortDescription}.");
				ctx.HuntInfo.Reset();
			}
			else
			{
				if (ctx.HuntInfo.TargetRoomId != target.Room.Id)
				{
					// Target moved, rebuild path
					if (!ctx.HuntInfo.SetTarget(ctx.Room, target))
					{
						ctx.Send($"{target.ShortDescription} can't be reached. The hunt is over.");
						return;
					}
				}

				var pr = ctx.HuntInfo.GetForRoom(ctx.Room.Id);
				if (pr == null)
				{
					// Should never happen
					ctx.HuntInfo.Reset();
					ctx.Send($"{target.ShortDescription} can't be reached. The hunt is over.");
					return;
				}

				var moveSteps = pr.Value.RemainingSteps;

				Debug.WriteLine($"{ctx.ShortDescription} hunts {target.ShortDescription}. Remaining steps: {moveSteps}");

				string farAway;
				if (moveSteps > 20)
				{
					farAway = "very far away";
				}
				else if (moveSteps > 10)
				{
					farAway = "far away";
				}
				else if (moveSteps > 5)
				{
					farAway = "not so far away";
				}
				else if (moveSteps > 2)
				{
					farAway = "close";
				}
				else
				{
					farAway = "very close";
				}

				ctx.Send($"You continue to hunt {target.ShortDescription}. The target is {farAway}.");

				try
				{
					ctx.SuppressStopHuntOnMovement = true;

					switch (pr.Value.Direction)
					{
						case Direction.North:
							BaseCommand.North.Execute(ctx);
							break;
						case Direction.East:
							BaseCommand.East.Execute(ctx);
							break;
						case Direction.South:
							BaseCommand.South.Execute(ctx);
							break;
						case Direction.West:
							BaseCommand.West.Execute(ctx);
							break;
						case Direction.Up:
							BaseCommand.Up.Execute(ctx);
							break;
						case Direction.Down:
							BaseCommand.Down.Execute(ctx);
							break;
					}

					if (target.Room.Id == ctx.Room.Id)
					{
						// Found
						ctx.Send($"The hunt is over. You found {target.ShortDescription}.");
						ctx.HuntInfo.Reset();
					}
				}
				finally
				{
					ctx.SuppressStopHuntOnMovement = false;
				}

				ctx.HuntInfo.LastHunt = now;
			}
		}

		private void WorldTick()
		{
			Fight.Process();

			// Creatures run
			var now = DateTime.Now;
			if (_lastRegenDt == null)
			{
				_lastRegenDt = now;
			}
			else if ((now - _lastRegenDt.Value).TotalMilliseconds >= 1000)
			{
				// Areas' repop
				foreach (var area in Area.Storage)
				{
					var passed = (float)(now - area.LastSpawn).TotalMinutes;

					if (passed > area.RespawnTimeInMinutes)
					{
						SpawnArea(area);
					}
				}

				var secondsPassed = (float)(now - _lastRegenDt.Value).TotalSeconds;

				// Process creature
				foreach (var creature in Creature.ActiveCreatures)
				{
					var ctx = creature.GetContext();

					// Hitpoints regen
					var currentValue = creature.State.Hitpoints;
					var fractionalValue = creature.State.FractionalHitpointsRegen;
					if (ProcessRegen(ref currentValue, creature.Stats.MaxHitpoints, ref fractionalValue,
						creature.Stats.GetHitpointsRegen(ctx.IsFighting), secondsPassed))
					{
						creature.State.Hitpoints = currentValue;
						creature.State.FractionalHitpointsRegen = fractionalValue;
					}

					// Mana regen
					currentValue = creature.State.Mana;
					fractionalValue = creature.State.FractionalManaRegen;
					if (ProcessRegen(ref currentValue, creature.Stats.MaxMana, ref fractionalValue,
						creature.Stats.GetManaRegen(ctx.IsFighting), secondsPassed))
					{
						creature.State.Mana = currentValue;
						creature.State.FractionalManaRegen = fractionalValue;
					}

					// Moves regen
					currentValue = creature.State.Moves;
					fractionalValue = creature.State.FractionalMovesRegen;
					if (ProcessRegen(ref currentValue, creature.Stats.MaxMoves, ref fractionalValue,
						creature.Stats.GetMovesRegen(ctx.IsFighting), secondsPassed))
					{
						creature.State.Moves = currentValue;
						creature.State.FractionalMovesRegen = fractionalValue;
					}

					// Remove expired effects
					_toDelete.Clear();
					foreach (var pair in creature.TemporaryAffects)
					{
						var ta = pair.Value;
						var passed = now - ta.Started;

						if (passed.TotalSeconds >= ta.Affect.DurationInSeconds.Value)
						{
							_toDelete.Add(pair.Key);
							ctx.Send($"'{ta.Name}' wears off.");
						}
					}

					foreach (var key in _toDelete)
					{
						creature.RemoveTemporaryAffect(key);
					}

					// Hunting
					ProcessHunt(ctx);

					// Command queue
					ctx.ProcessCommandQueue();
				}

				// Process characters
				foreach (var character in Character.ActiveCharacters)
				{
					// Autoskill
					var ctx = (Commands.ExecutionContext)character.Tag;
					if (ctx.IsFighting && !ctx.WaitingCommandLag() && !string.IsNullOrEmpty(character.FightSkill))
					{
						var command = BaseCommand.FindCommand(character.FightSkill);
						if (command != null)
						{
							var cost = command.CalculateCost(ctx);
							if (cost.Hitpoints < ctx.State.Hitpoints &&
								cost.Mana < ctx.State.Mana &&
								cost.Moves < ctx.State.Moves)
							{
								ctx.SendInfoMessage($"Performing autoskill {character.FightSkill}");
								if (!command.Execute(ctx))
								{
									ctx.ParseAndExecute("autoskill off");
								}
							}
						}
					}
				}

				_lastRegenDt = now;
			}
		}

		public void SpawnArea(Area area, Action<string> spawnLogger = null)
		{
			var count = 0;
			foreach (var mobileReset in area.MobileResets)
			{
				if (mobileReset.MobileInstance != null)
				{
					continue;
				}

				var mobile = Mobile.GetMobileById(mobileReset.MobileId);
				if (mobile == null)
				{
					Logger.Warn($"{area.Name}: Couldn't find mobile with id {mobileReset.MobileId}");
					continue;
				}

				var room = Room.GetRoomById(mobileReset.RoomId);
				if (room == null)
				{
					Logger.Warn($"{area.Name}: Couldn't find room with id {mobileReset.RoomId}");
					continue;
				}

				// Spawn
				var newMobile = new MobileInstance(mobile, room, mobileReset);
				new Commands.ExecutionContext(newMobile);

				if (spawnLogger != null)
				{
					spawnLogger($"Spawned {newMobile} at {room}");
				}

				++count;
			}

			var str = $"Spawned {count} mobiles for area {area}";
			Logger.Info(str);

			if (spawnLogger != null)
			{
				spawnLogger(str);
			}

			area.LastSpawn = DateTime.Now;
		}

		public void Start(string dataFolder)
		{
			try
			{
				LoadDatabase(dataFolder);

				Logger.Info("Spawning areas");
				foreach (var area in Area.Storage)
				{
					SpawnArea(area);
				}

				Logger.Info("Starting WebService");
				_webService.Start();

				Logger.Info("Creating Socket Listener at port {0}", Configuration.ServerPort);

				var ipEndPoint = new IPEndPoint(IPAddress.Any, Configuration.ServerPort);
				var sListener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				sListener.Bind(ipEndPoint);
				sListener.Listen(10);

				sListener.BeginAccept(EndAccept, sListener);

				while (true)
				{
					try
					{
						Debug.WriteLine("Tick");

						// Process new connections
						lock (_newConnections)
						{
							if (_newConnections.Count > 0)
							{
								foreach (var connection in _newConnections)
								{
									var session = new Session(connection, Sessions.Length == 0);
									session.Disconnected += session_Disconnected;
									_sessions.Add(session);
								}

								Logger.Info($"Active sessions: {_sessions.Count}");

								_newConnections.Clear();
							}
						}

						// Process player input
						foreach (var session in Sessions)
						{
							session.ProcessInput();
						}

						WorldTick();

						// Flush outputs
						foreach (var session in Sessions)
						{
							session.FlushOutput();
						}

						// Sleep
						_mainThreadEvent.WaitOne(1000);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						Logger.Error(ex);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				throw;
			}
		}

		void EndAccept(IAsyncResult result)
		{
			var socket = result.AsyncState as Socket;

			try
			{
				var handler = socket.EndAccept(result);

				// Incoming connection
				var remote = (IPEndPoint)handler.RemoteEndPoint;
				Logger.Info("Incoming connection from {0}:{1}", remote.Address, remote.Port);
				var connection = new Connection(handler);

				lock (_newConnections)
				{
					_newConnections.Add(connection);
				}

				Awake();
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}

			try
			{
				socket.BeginAccept(EndAccept, socket);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}

		void session_Disconnected(object sender, EventArgs e)
		{
			try
			{
				var session = (Session)sender;
				var connection = session.Connection;
				Logger.Info("Closed connection from {0}:{1}", connection.RemoteIp, connection.RemotePort);

				lock (_sessions)
				{
					_sessions.Remove(session);
					Logger.Info($"Active sessions: {_sessions.Count}");
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}

		public void Awake() => _mainThreadEvent.Set();
	}
}