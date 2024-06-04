using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using AbarimMUD.Data;
using AbarimMUD.Utils;
using NLog;

namespace AbarimMUD.WebService
{
	public class Service
	{
		private const string AccountKey = "account";
		private const string PasswordKey = "password";
		private const string AreaKey = "area";

		private static readonly Logger _logger = LogManager.GetLogger("WebService");

		private bool _running = true;
		private readonly HttpListener _listener = new HttpListener();

		public void Start()
		{
			ThreadPool.QueueUserWorkItem(MainThreadProc);
		}

		private Character GetAdminCharacter(NameValueCollection parameters, out ResultDescription result)
		{
			result = null;

			var account = parameters.Get(AccountKey);
			if (string.IsNullOrEmpty(account))
			{
				result = ResultDescription.CreateRequiredParameterIsNotSet("account");
				return null;
			}

			var password = parameters.Get(PasswordKey);
			if (string.IsNullOrEmpty(password))
			{
				result = ResultDescription.CreateRequiredParameterIsNotSet("password");
				return null;
			}

			// Find account
			var acc = Account.GetAccountByName(account);
			if (acc == null)
			{
				result = ResultDescription.CreateInvalidAccount(account);
				return null;
			}

			var inputHash = HashUtils.CalculateMD5Hash(password);
			if (inputHash != acc.PasswordHash)
			{
				result = ResultDescription.CreateFromResultType(ResultType.WrongPassword);
				return null;
			}

			var characters = Character.GetCharactersByAccountName(acc.Name);
			var admin = (from c in characters where c.IsStaff select c).FirstOrDefault();
			if (admin == null)
			{
				result = ResultDescription.CreateFromResultType(ResultType.NotAdmin);
				return null;
			}

			return admin;
		}

		public string GetVersion()
		{
			return Assembly.GetExecutingAssembly().GetName().Version.ToString().SerializeToJSON();
		}

		public string ListAreas()
		{
			var result = new ListAreasResult();

			try
			{
				do
				{
					var areas = Area.Storage.ToArray();
					if (areas.Length == 0)
					{
						result.Result = ResultDescription.CreateFromResultType(ResultType.NoMaps);
						break;
					}

					// Build infos
					var areaInfos = new List<ListAreasResult.AreaInfo>();
					foreach (var a in areas)
					{
						areaInfos.Add(new ListAreasResult.AreaInfo
						{
							Id = a.Filename,
							Name = a.Name
						});
					}

					result.Result = ResultDescription.CreateFromResultType(ResultType.OK);
					result.Areas = areaInfos.ToArray();
				} while (false);
			}
			catch (Exception ex)
			{
				result.Result = ResultDescription.CreateFromResultType(ResultType.UnrecognizedError);
				_logger.Info(ex);
			}

			return result.SerializeToJSON();
		}

		private string GetArea(string areaId)
		{
			var result = new GetAreaResult();
			try
			{
				do
				{
					var area = Area.GetAreaByName(areaId);
					if (area == null)
					{
						result.Result = ResultDescription.CreateAreaNotFound(areaId);
						break;
					}

					result.Result = ResultDescription.CreateFromResultType(ResultType.OK);
					result.Area = area;
				} while (false);
			}
			catch (Exception ex)
			{
				result.Result = ResultDescription.CreateFromResultType(ResultType.UnrecognizedError);
				_logger.Info(ex);
			}

			return result.SerializeToJSON();
		}

		private void ProcessRequest(HttpListenerContext context)
		{
			var request = context.Request;

			if (request == null)
			{
				_logger.Info("Request is null");
				return;
			}

			_logger.Info("Incoming request: {0}", request);

			if (request.Url == null || request.Url.Segments.Length < 1)
			{
				_logger.Info("Request.Url is empty");
				return;
			}

			var method = request.Url.Segments[2].ToLower().Replace("/", string.Empty);
			_logger.Info("method: {0}", method);

			var data = string.Empty;
			switch (method)
			{
				case "version":
					data = GetVersion();
					break;

				case "listareas":
					data = ListAreas();
					break;

				case "getarea":
					data = GetArea(request.Url.Segments[3].ToLower());
					break;
			}

			var buffer = Encoding.UTF8.GetBytes(data);

			// Write response
			var response = context.Response;
			response.ContentLength64 = buffer.Length;
			var output = response.OutputStream;

			output.Write(buffer, 0, buffer.Length);
			output.Close();
		}

		private void MainThreadProc(object state)
		{
			try
			{
				_listener.Prefixes.Add(Configuration.WebServiceUrl);
				_listener.Start();

				_logger.Info($"WebService Started at '{Configuration.WebServiceUrl}'");

				while (_running)
				{
					try
					{
						var context = _listener.GetContext();

						ProcessRequest(context);
					}
					catch (Exception ex)
					{
						_logger.Info(ex);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Info(ex);
			}
		}
	}
}