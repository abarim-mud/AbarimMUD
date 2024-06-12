using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;

namespace AbarimMUD
{
	public sealed class Connection
	{
		public static class ConsoleCommand
		{
			private static readonly Regex CommandRegex = new Regex(@"\[(\w+)\]");

			private static readonly Dictionary<string, string> _commandsByNames = new Dictionary<string, string>();

			public static readonly string NewLine = "\n\r";
			public static readonly string ColorClear = Convert.ToChar(27) + "[0m";
			public static readonly string Bold = Convert.ToChar(27) + "[1m";
			public static readonly string Faint = Convert.ToChar(27) + "[2m";
			public static readonly string Underline = Convert.ToChar(27) + "[4m";
			public static readonly string Blink = Convert.ToChar(27) + "[5m";
			public static readonly string Reverse = Convert.ToChar(27) + "[7m";
			public static readonly string ForeColorBlack = Convert.ToChar(27) + "[0;30m";
			public static readonly string ForeColorRed = Convert.ToChar(27) + "[0;31m";
			public static readonly string ForeColorGreen = Convert.ToChar(27) + "[0;32m";
			public static readonly string ForeColorYellow = Convert.ToChar(27) + "[0;33m";
			public static readonly string ForeColorBlue = Convert.ToChar(27) + "[0;34m";
			public static readonly string ForeColorMagenta = Convert.ToChar(27) + "[0;35m";
			public static readonly string ForeColorCyan = Convert.ToChar(27) + "[0;36m";
			public static readonly string ForeColorLightGrey = Convert.ToChar(27) + "[0;37m";
			public static readonly string ForeColorDarkGrey = Convert.ToChar(27) + "[1;30m";
			public static readonly string ForeColorLightRed = Convert.ToChar(27) + "[1;31m";
			public static readonly string ForeColorLightGreen = Convert.ToChar(27) + "[1;32m";
			public static readonly string ForeColorLightYellow = Convert.ToChar(27) + "[1;33m";
			public static readonly string ForeColorLightBlue = Convert.ToChar(27) + "[1;34m";
			public static readonly string ForeColorLightMagenta = Convert.ToChar(27) + "[1;35m";
			public static readonly string ForeColorLightCyan = Convert.ToChar(27) + "[1;36m";
			public static readonly string ForeColorWhite = Convert.ToChar(27) + "[1;37m";
			public static readonly string BgBlack = Convert.ToChar(27) + "[40m";
			public static readonly string BgRed = Convert.ToChar(27) + "[41m";
			public static readonly string BgGreen = Convert.ToChar(27) + "[42m";
			public static readonly string BgYellow = Convert.ToChar(27) + "[43m";
			public static readonly string BgBlue = Convert.ToChar(27) + "[44m";
			public static readonly string BgMagenta = Convert.ToChar(27) + "[45m";
			public static readonly string BgCyan = Convert.ToChar(27) + "[46m";
			public static readonly string BgWhite = Convert.ToChar(27) + "[47m";

			static ConsoleCommand()
			{
				var staticFields = typeof(ConsoleCommand).GetFields(BindingFlags.Public | BindingFlags.Static);
				foreach (var field in staticFields)
				{
					if (field.FieldType != typeof(string))
					{
						continue;
					}

					var name = field.Name.ToLower();
					if (name.StartsWith("forecolor"))
					{
						name = name.Substring(9);
					}

					if (name == "colorclear")
					{
						name = "clear";
					}

					_commandsByNames[name] = (string)field.GetValue(null);
				}
			}

			public static string UpdateCommand(string source)
			{
				var replaces = new Dictionary<string, string>();

				var matches = CommandRegex.Matches(source);
				foreach (Match match in matches)
				{
					var s = match.Groups[1].Value.ToLower();

					string command;
					if (replaces.ContainsKey(s) || !_commandsByNames.TryGetValue(s, out command))
					{
						continue;
					}

					replaces[s] = command;
				}

				foreach(var pair in replaces)
				{
					source = source.Replace("[" + pair.Key + "]", pair.Value);
				}

				return source;
			}
		}

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

			data = FixNewLines(data);
			data = ConsoleCommand.UpdateCommand(data);

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

		private static string FixNewLines(string data)
		{
			// Firstly determine if line contains '\n' not followed by '\r'

			// Convert all '\n' to '\n\r'
			var toFix = false;
			for (var i = 0; i < data.Length; ++i)
			{
				if (data[i] == '\n' &&
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

			var sb = new StringBuilder(data.Length * 2);
			for (var i = 0; i < data.Length; ++i)
			{
				sb.Append(data[i]);

				if (data[i] == '\n' &&
					((i >= data.Length - 1) ||
					(data[i + 1] != '\r')))
				{
					sb.Append('\r');
				}
			}

			return sb.ToString();
		}
	}
}