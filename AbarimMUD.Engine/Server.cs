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

namespace AbarimMUD
{
	public sealed class Server
	{
		private static readonly Server _instance = new Server();
		private readonly ObservableCollection<Session> _sessions = new ObservableCollection<Session>();
		private Session[] _sessionsCopy = null;
		private readonly AutoResetEvent _mainThreadEvent = new AutoResetEvent(false);
		private readonly Service _webService = new Service();

		public static Logger Logger { get; private set; } = LogManager.GetLogger("Logs/Server");

		public static Server Instance
		{
			get { return _instance; }
		}

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

		private void LoadDatabase()
		{
			Logger.Info("Loading Database");

			DataContext.Initialize(Configuration.DataFolder, Logger.Info);

			DataContext.Register(Race.Storage);
			DataContext.Register(GameClass.Storage);
			DataContext.Register(Skill.Storage);
			DataContext.Register(Area.Storage);
			DataContext.Register(Account.Storage);
			DataContext.Register(Character.Storage);
			DataContext.Register(Social.Storage);

			DataContext.Load();

			GameClass.Storage.SaveAll();
		}

		public void Start()
		{
			try
			{
				LoadDatabase();

				if (Area.Storage.Count == 0)
				{
					Logger.Info("No areas exist. Creating test area with single room.");
					var newArea = new Area
					{
						Name = "Test Area"
					};

					// Create new room
					var newRoom = new Room
					{
						Id = Area.NextRoomId,
						Name = "Empty",
						Description = "Empty"
					};

					newArea.Rooms.Add(newRoom);
					newArea.Save();

					Configuration.StartRoomId = newRoom.Id;
				}

				Logger.Info("Spawning areas");
				foreach (var area in Area.Storage)
				{
					foreach (var areaReset in area.Resets)
					{
						if (areaReset.ResetType != Data.AreaResetType.NPC)
						{
							continue;
						}

						var mobile = Area.EnsureMobileById(areaReset.Id1);
						var room = Area.EnsureRoomById(areaReset.Id2);

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

						// Process player input
						foreach (var session in Sessions)
						{
							session.ProcessInput();
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
				var session = new Session(connection, Sessions.Length == 0);

				session.Disconnected += session_Disconnected;

				lock (_sessions)
				{
					_sessions.Add(session);
				}
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