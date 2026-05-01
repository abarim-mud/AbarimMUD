using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using AbarimMUD.WebService;
using System.Collections.ObjectModel;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using System.Collections.Generic;
using AbarimMUD.Combat;
using System.Threading;
using Ur;
using AbarimMUD.Services;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class Server
	{
		private const int PulseInMs = 250;

		private static readonly Server _instance = new Server();
		private readonly List<Connection> _newConnections = new List<Connection>();
		private readonly ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
		private Session[] _sessionsCopy = null;
		private readonly AutoResetEvent _mainThreadEvent = new AutoResetEvent(false);
		private readonly Service _webService = new Service();
		private readonly List<BaseService> _services = new List<BaseService>();

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

			UrContext.Load(dataFolder);

			if (Configuration.SaveDataOnStart)
			{
				// Fix ids
				foreach (var area in Area.Storage)
				{
					var id = area.StartId;
					foreach (var room in area.Rooms)
					{
						room.Id = id;
						++id;
					}

					id = area.StartId;
					foreach (var mobile in area.Mobiles)
					{
						mobile.Id = id;
						++id;
					}
				}

				Area.Storage.SaveAll();
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
				LevelInfo.Storage.SaveAll();
			}
		}

		private void WorldTick()
		{
			Fight.Process();

			foreach (var service in _services)
			{
				service.Update();
			}
		}

		public Session GetSessionByCharacter(string name)
		{
			return (from s in Sessions where s.Character != null && s.Character.Name.EqualsToIgnoreCase(name) select s).FirstOrDefault();
		}

		public void SpawnArea(Area area, Action<string> spawnLogger = null)
		{
			var count = 0;

			foreach (var room in area.Rooms)
			{
				foreach (var mobileSpawn in room.MobileSpawns)
				{
					if (mobileSpawn.Instance != null)
					{
						continue;
					}

					// Spawn
					var newMobile = new MobileInstance(mobileSpawn);
					new Commands.ExecutionContext(newMobile);

					if (spawnLogger != null)
					{
						spawnLogger($"Spawned {newMobile} at {room}");
					}

					++count;
				}
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

				// Create services
				Logger.Info("Creating services");
				_services.Add(new ActivityService());
				_services.Add(new FightskillService());
				_services.Add(new MobileAggroService());
				_services.Add(new MobileWanderService());
				_services.Add(new RespawnAreasService());

				// Spawn areas
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
						var now = DateTime.Now;

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
						var passed = (int)(DateTime.Now - now).TotalMilliseconds;
						_mainThreadEvent.WaitOne(PulseInMs - passed);
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