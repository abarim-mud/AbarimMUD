using System;
using System.Net;
using System.Net.Sockets;
using NLog;
using AbarimMUD.WebService;
using System.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using AbarimMUD.Data;
using AbarimMUD.Storage;
using System.Collections.Generic;

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
		private readonly List<Fight> _fights = new List<Fight>();
		private DateTime? _lastRegenDt;

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

		public IReadOnlyList<Fight> Fights => _fights;

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

			DataContext.Initialize(dataFolder, Logger.Info);

			DataContext.Register(Configuration.Storage);
			DataContext.Register(LevelInfo.Storage);
			DataContext.Register(Item.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(GameClass.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();

			Configuration.Save();
			/*			long minimumXp = 2000;
						long maximumXp = 20000000000;
						int levels = 100;

						var values = new Dictionary<int, long>();
						for (var level = 1; level <= levels; ++level)
						{
							var k = (float)(level - 1) / (levels - 1);

							k *= (k * k * k * k);
							var xp = (long)(minimumXp + (maximumXp - minimumXp) * k);

							// Round up to 3 first numbers
							var numbers = xp.ToString().Length;

							var d = xp / Math.Pow(10, numbers - 3);
							d = Math.Round(d);

							// Get rid of trailing 1 or 9
							xp = (long)d;

							var moves = 0;
							while (xp > 10)
							{
								var lastNum = xp % 10;
								if (lastNum == 1)
								{
									xp -= 1;
								}
								else if (lastNum == 9)
								{
									xp += 1;
								}
								else
								{
									break;
								}

								++moves;
								xp /= 10;
							}

							xp *= (long)Math.Pow(10, numbers + moves - 3);
							values[level] = xp;
						}

						foreach(var pair in values)
						{
							LevelInfo.Storage.Create(new LevelInfo(pair.Key, pair.Value));
						}*/

			/*			LevelInfo.Storage.SaveAll();

						var thief = GameClass.EnsureClassById("thief");

						var eqSet = new EqSet
						{
							MinimumLevel = 1,
							Items = new Item[]
							{
								Item.EnsureItemById("ironDagger"),
								Item.EnsureItemById("leatherHelm"),
								Item.EnsureItemById("leatherArmor"),
								Item.EnsureItemById("leatherPants"),
								Item.EnsureItemById("leatherGloves"),
							}
						};

						thief.EqSets = new EqSet[] { eqSet };

						GameClass.Storage.SaveAll();*/
			GameClass.Storage.SaveAll();
		}

		private void WorldTick()
		{
			// Process fights
			foreach (var fight in _fights)
			{
				fight.DoRound();
			}

			// Remove finished fights
			_fights.RemoveAll(f => f.Finished);

			// Creatures run
			var now = DateTime.Now;
			if (_lastRegenDt == null)
			{
				_lastRegenDt = now;
			}
			else if ((now - _lastRegenDt.Value).TotalMilliseconds >= 1000)
			{
				var secondsPassed = (float)(now - _lastRegenDt.Value).TotalSeconds;

				// Process creature
				foreach (var creature in Creature.AllCreatures)
				{
					if (creature.State.Hitpoints >= creature.Stats.MaxHitpoints)
					{
						continue;
					}

					var hpRegen = creature.Stats.HitpointsRegen * secondsPassed / 60.0f;

					creature.State.FractionalRegen += hpRegen;

					if (creature.State.FractionalRegen > 1)
					{
						// Update real hp
						var hpUpdate = (int)creature.State.FractionalRegen;
						creature.State.Hitpoints += hpUpdate;
						creature.State.FractionalRegen -= hpUpdate;

						if (creature.State.Hitpoints >= creature.Stats.MaxHitpoints)
						{
							// Full
							creature.State.Hitpoints = creature.Stats.MaxHitpoints;
							creature.State.FractionalRegen = 0;
						}
					}
				}

				_lastRegenDt = now;
			}
		}

		public void Start(string dataFolder)
		{
			try
			{
				LoadDatabase(dataFolder);

				Logger.Info("Spawning areas");
				foreach (var area in Area.Storage)
				{
					foreach (var mobileReset in area.MobileResets)
					{
						var mobile = Mobile.EnsureMobileById(mobileReset.MobileId);
						var room = Room.EnsureRoomById(mobileReset.RoomId);

						// Spawn
						var newMobile = new MobileInstance(mobile)
						{
							Room = room
						};
					}
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

		public void StartFight(Room room, Creature attacker, Creature target)
		{
			var fight = new Fight(room, attacker, target);
			fight.DoRound();

			_fights.Add(fight);
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