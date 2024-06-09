using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NLog;

namespace AbarimMUD
{
	public sealed class Connection
	{
		private readonly Socket _socket;
		private readonly IPAddress _remoteIp;
		private readonly int _remotePort;
		private readonly byte[] _buffer = new byte[1024];
		private readonly StringBuilder _inputBuffer = new StringBuilder();
		private readonly List<string> _sendQueue = new List<string>();
		private readonly AutoResetEvent _sendEvent = new AutoResetEvent(false);

		private bool _alive = true;
		private string _input;

		public bool Alive
		{
			get => _alive;

			private set
			{
				if (value == _alive)
				{
					return;
				}

				_alive = value;
				if (!_alive)
				{
					_sendEvent.Set();
				}
			}
		}

		public IPAddress RemoteIp => _remoteIp;
		public int RemotePort => _remotePort;

		public string LoggerName => RemoteIp.ToString();

		public Logger Logger { get; set; }

		public Connection(Socket socket)
		{
			if (socket == null)
			{
				throw new ArgumentNullException("socket");
			}

			_socket = socket;

			var remote = (IPEndPoint)_socket.RemoteEndPoint;
			_remoteIp = remote.Address;
			_remotePort = remote.Port;

			// Using server logger as default
			Logger = Server.Logger;

			// Begin receive	
			_socket.BeginReceive(_buffer, 0, _buffer.Length, 0, ReadCallback, null);

			ThreadPool.QueueUserWorkItem(SendProc);
		}

		private void ReadCallback(IAsyncResult ar)
		{
			try
			{
				string content;

				// Retrieve the state object and the handler socket
				// from the asynchronous state object.

				// Read data from the client socket. 
				var bytesRead = _socket.EndReceive(ar);
				do
				{
					if (bytesRead <= 0)
					{
						Alive = false;
						break;
					}
					var rawData = Encoding.UTF8.GetString(_buffer, 0, bytesRead);
					if (rawData.Contains("Core.Supports.Set"))
					{
						// Ignore this data, as it comes from mudlet bug
						break;
					}
					_inputBuffer.Append(Encoding.ASCII.GetString(_buffer, 0, bytesRead));

					// Check for end-of-file tag. If it is not there, read 
					// more data.
					content = _inputBuffer.ToString();
					var rlPos = content.IndexOf("\n");
					if (rlPos == -1)
					{
						break;
					}

					// '\n' ends a command
					var data = content.Substring(0, rlPos).Trim();

					Logger.Info("Incomming data: '{0}'", data);

					_input = data;
					Server.Instance.Awake();

					// Reset input buffer
					_inputBuffer.Clear();
				} while (false);

				if (_alive)
				{
					_socket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReadCallback), null);
				}
				else
				{
					Logger.Info("Connection has been closed by the remote host");
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				Alive = false;
			}
		}

		public string GetInput()
		{
			var result = _input;
			_input = null;

			return result;
		}

		public void Send(string data)
		{
			if (!_alive)
			{
				Logger.Warn("Try to send to disconnected client: '{0}'", data);
				return;
			}

			// Use dedicated proc to send message to the client
			// So on one hand sending wont freeze the main loop
			// On the other hand - the messages would send in the same order they are queued
			// The second reason explains why we dont use SendAsync
			Logger.Info("Queing message to client: '{0}'", data);
			lock (_sendQueue)
			{
				_sendQueue.Add(data);
			}

			_sendEvent.Set();
		}

		private void SendProc(object obj)
		{
			while (_alive)
			{
				try
				{
					string[] toSend;
					lock (_sendQueue)
					{
						toSend = _sendQueue.ToArray();
						_sendQueue.Clear();
					}

					if (toSend != null)
					{
						for (var i = 0; i < toSend.Length; ++i)
						{
							var result = Encoding.UTF8.GetBytes(toSend[i]);
							_socket.Send(result);
						}
					}

					_sendEvent.WaitOne();
				}
				catch (Exception ex)
				{
					Logger.Error(ex);
				}
			}
		}

		public void Disconnect()
		{
			Logger.Info("Closing connection...");
			Alive = false;
			_socket.Disconnect(true);
			Logger.Info("Connection has been closed");
		}
	}
}