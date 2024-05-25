using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NLog;
using AbarimMUD.WebService;
using AbarimMUD.Utils;
using System.Threading;
using System.Diagnostics;

namespace AbarimMUD
{
	public sealed class Server
	{
		private static readonly Logger _logger = LogUtility.GetGlobalLogger();
		private static readonly Server _instance = new Server();
		private readonly List<Session> _sessions = new List<Session>();
		private readonly List<Session> _sessionsCopy = new List<Session>();
		private readonly AutoResetEvent _mainThreadEvent = new AutoResetEvent(false);
		private readonly Service _webService = new Service();

		public static Server Instance
		{
			get { return _instance; }
		}

		public Session[] Sessions
		{
			get { return _sessions.ToArray(); }
		}

		private Server()
		{
		}

		public void Start()
		{
			try
			{
				_logger.Info("Starting Server");
				Database.Initialize();

				_logger.Info("Starting WebService");
				_webService.Start();

				_logger.Info("Creating Socket Listener at port {0}", Configuration.ServerPort);

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
						_sessionsCopy.Clear();
						lock (_sessions)
						{
							foreach (var session in _sessions)
							{
								_sessionsCopy.Add(session);
							}
						}

						foreach (var session in _sessionsCopy)
						{
							session.ProcessInput();
						}

						// Sleep
						_mainThreadEvent.WaitOne(1000);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
						_logger.Error(ex);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				_logger.Error(ex);
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
				_logger.Info("Incoming connection from {0}:{1}", remote.Address, remote.Port);
				var connection = new Connection(handler);
				var session = new Session(connection);

				session.Disconnected += session_Disconnected;

				lock (_sessions)
				{
					_sessions.Add(session);
				}
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}

			try
			{
				socket.BeginAccept(EndAccept, socket);
			}
			catch (Exception ex)
			{
				_logger.Error(ex);
			}
		}

		void session_Disconnected(object sender, EventArgs e)
		{
			var session = (Session)sender;
			var connection = session.Connection;
			_logger.Info("Closed connection from {0}:{1}", connection.RemoteIp, connection.RemotePort);

			lock (_sessions)
			{
				_sessions.Remove(session);
			}
		}

		public void Awake() => _mainThreadEvent.Set();
	}
}