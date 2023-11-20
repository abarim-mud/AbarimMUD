using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NLog;
using AbarimMUD.Data;
using AbarimMUD.WebService;
using AbarimMUD.Utils;

namespace AbarimMUD
{
	public sealed class Server
	{
		private static readonly Logger _logger = LogUtility.GetGlobalLogger();
		private static readonly Server _instance = new Server();
		private readonly List<Session> _sessions = new List<Session>();
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

				_logger.Info("Starting WebService");
				_webService.Start();

				_logger.Info("Creating Socket Listener at port {0}", Configuration.ServerPort);

				var ipEndPoint = new IPEndPoint(IPAddress.Any, Configuration.ServerPort);
				var sListener = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				sListener.Bind(ipEndPoint);
				sListener.Listen(10);

				while (true)
				{
					_logger.Info("Waiting for a connection");
					var handler = sListener.Accept();

					// Incoming connection
					var remote = (IPEndPoint) handler.RemoteEndPoint;
					_logger.Info("Incoming connection from {0}:{1}", remote.Address, remote.Port);
					var connection = new Connection(handler);
					var session = new Session(connection);

					session.Disconnected += session_Disconnected;

					_sessions.Add(session);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				_logger.Error(ex);
			}
		}

		void session_Disconnected(object sender, EventArgs e)
		{
			var session = (Session) sender;
			var remote = (IPEndPoint) session.Connection.Socket.RemoteEndPoint;
			_logger.Info("Closed connection from {0}:{1}", remote.Address, remote.Port);

			_sessions.Remove(session);
		}
	}
}