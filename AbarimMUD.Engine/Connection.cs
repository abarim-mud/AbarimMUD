using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using AbarimMUD.Utils;
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
		private string _input;

		public bool Alive { get; private set; } = true;

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

				if (Alive)
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
			if (!Alive)
			{
				Logger.Warn("Try to send to disconnected client: '{0}'", data);
				return;
			}

			data = AsciiRichText.Format(data);
			data = FixNewLines(data);
			if (string.IsNullOrEmpty(data))
			{
				return;
			}

			Logger.Info("Sending message to client: '{0}'", data);
			var result = Encoding.UTF8.GetBytes(data);
			if (result.Length == 0)
			{
				return;
			}

			try
			{
				_socket.BeginSend(result, 0, result.Length, SocketFlags.None, SendCallback, null);
			}
			catch (Exception ex)
			{
				Disconnect();
				Logger.Error(ex);
			}
		}

		private void SendCallback(IAsyncResult result)
		{
			try
			{
				_socket.EndSend(result);
			}
			catch (Exception ex)
			{
				Disconnect();
				Logger.Error(ex);
			}
		}

		public void Disconnect()
		{
			if (_socket.Connected)
			{
				return;
			}

			Logger.Info("Closing connection...");
			Alive = false;
			_socket.Disconnect(true);
			Logger.Info("Connection has been closed");
		}

		private static string FixNewLines(string data)
		{
			// Firstly determine if line contains '\n' not followed by '\r'
			// Convert all '\n' to '\n\r'
			var toFix = false;
			for (var i = 0; i < data.Length; ++i)
			{
				var c = data[i];

				// Check if there's '\r' not preceeded by '\n'
				if (c == '\r' &&
					(i == 0 || data[i - 1] != 'n'))
				{
					toFix = true;
					break;
				}

				// Check if there's '\n' not followed by '\r'
				if (c == '\n' &&
					((i >= data.Length - 1) ||
					(data[i + 1] != '\r')))
				{
					toFix = true;
					break;
				}
			}

			if (!toFix)
			{
				// Doesnt need to be fixed, return original
				return data;
			}

			// Make sure every '\n' is followed by '\r'
			var sb = new StringBuilder(data.Length * 2);
			for (var i = 0; i < data.Length; ++i)
			{
				var c = data[i];

				if (c == '\r')
				{
					continue;
				}

				sb.Append(data[i]);
				if (c == '\n')
				{
					sb.Append('\r');
				}
			}

			return sb.ToString();
		}
	}
}